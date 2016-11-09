using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using log4net;
using RoboticsTxt.Lib.Commands;
using RoboticsTxt.Lib.Components.Communicator;
using RoboticsTxt.Lib.Contracts;

namespace RoboticsTxt.Lib.Components.Sequencer
{
    public class MotorPositionController : INotifyPropertyChanged, IDisposable
    {
        private readonly ControllerCommunicator controllerCommunicator;
        private readonly ControllerSequencer controllerSequencer;
        private readonly MotorDistanceInfo motorDistanceInfo;
        private Direction? currentDirection;

        private int currentPosition;
        private readonly ISubject<int> positionChangesSubject;

        private bool currentReferenceState;
        private ILog logger;

        public MotorConfiguration MotorConfiguration { get; }

        public int CurrentPosition
        {
            get { return currentPosition; }
            set
            {
                if (currentPosition == value)
                {
                    return;
                }

                currentPosition = value;
                OnPropertyChanged();
            }
        }

        public IObservable<int> PositionChanges => this.positionChangesSubject.AsObservable();

        internal MotorPositionController(MotorConfiguration motorConfiguration,
            ControllerCommunicator controllerCommunicator, ControllerSequencer controllerSequencer)
        {
            this.controllerCommunicator = controllerCommunicator;
            this.controllerSequencer = controllerSequencer;
            MotorConfiguration = motorConfiguration;

            this.logger = LogManager.GetLogger(typeof (MotorPositionController));

            motorDistanceInfo = controllerCommunicator.MotorDistanceInfos.First(m => m.Motor == motorConfiguration.Motor);
            motorDistanceInfo.DistanceDifferences.Subscribe(diff =>
            {
                if (currentDirection == null)
                {
                    return;
                }

                if (currentDirection.Value == motorConfiguration.ReferencingDirection)
                {
                    Interlocked.Add(ref currentPosition, -diff);
                    OnPropertyChanged(nameof(CurrentPosition));
                }
                else
                {
                    Interlocked.Add(ref currentPosition, diff);
                    OnPropertyChanged(nameof(CurrentPosition));
                }

                this.positionChangesSubject.OnNext(this.currentPosition);
            });

            this.controllerCommunicator.UniversalInputs[(int) motorConfiguration.ReferencingInput].StateChanges
                .Subscribe(s => this.currentReferenceState = s);

            this.controllerCommunicator.UniversalInputs[(int) motorConfiguration.ReferencingInput].StateChanges.Where(
                s => s == this.MotorConfiguration.ReferencingInputState)
                .Subscribe(
                    s =>
                    {
                        if (this.motorDistanceInfo.IsTracking)
                        {
                            this.StopMotor();
                        }
                    });

            this.positionChangesSubject = new Subject<int>();
        }

        /// <summary>
        /// Starts the <see cref="Motor"/> specified in the <see cref="MotorConfiguration"/> of the controller immediately.
        /// </summary>
        /// <param name="speed">The speed of the motor.</param>
        /// <param name="direction">The direction to movement.</param>
        public async Task StartMotorAsync(Speed speed, Direction direction)
        {
            if (direction == this.MotorConfiguration.ReferencingDirection &&
                this.currentReferenceState == this.MotorConfiguration.ReferencingInputState)
            {
                return;
            }

            await this.StartMotorAndMoveDistanceAsync(speed, direction, (short) this.GetAvailableDistance(direction));
        }

        /// <summary>
        /// Stops the <see cref="Motor"/> specified in the <see cref="MotorConfiguration"/> of the controller immediately.
        /// </summary>
        public void StopMotor()
        {
            this.controllerCommunicator.QueueCommand(new StopMotorCommand(MotorConfiguration.Motor));
        }

        /// <summary>
        /// Starts the <see cref="Motor"/> specified in the <see cref="MotorConfiguration"/> immediately and runs the specified <paramref name="distance"/>.
        /// </summary>
        /// <param name="speed">The speed of the motor.</param>
        /// <param name="direction">The direction to start.</param>
        /// <param name="distance">The distance to run.</param>
        /// <param name="waitForCompletion"></param>
        public async Task StartMotorAndMoveDistanceAsync(Speed speed, Direction direction, short distance,
            bool waitForCompletion = false)
        {
            if (direction == this.MotorConfiguration.ReferencingDirection &&
                this.currentReferenceState == this.MotorConfiguration.ReferencingInputState)
            {
                return;
            }

            if (direction != this.MotorConfiguration.ReferencingDirection)
            {
                var availableDistance = this.GetAvailableDistance(direction);

                if (availableDistance <= 0)
                {
                    return;
                }

                if (distance > availableDistance)
                {
                    distance = (short) availableDistance;
                }
            }

            currentDirection = direction;
            var motorRunDistanceCommand = new MotorRunDistanceCommand(MotorConfiguration.Motor, speed, direction,
                distance);
            controllerCommunicator.QueueCommand(motorRunDistanceCommand);

            if (waitForCompletion)
            {
                await motorRunDistanceCommand.WaitForCompletionAsync();
            }
        }

        /// <summary>
        /// Moves the <see cref="Motor"/> specified in the <see cref="MotorConfiguration"/> to the given <see cref="MotorPositionInfo"/>.
        /// </summary>
        /// <param name="motorPositionInfo">Target position.</param>
        public async Task MoveMotorToPositionAsync([NotNull] MotorPositionInfo motorPositionInfo)
        {
            if (motorPositionInfo == null) throw new ArgumentNullException(nameof(motorPositionInfo));

            var targetPosition = motorPositionInfo.Position;

            if (targetPosition > this.MotorConfiguration.Limit)
            {
                throw new InvalidOperationException("Position would breach limit.");
            }

            var distanceToPosition = targetPosition - this.CurrentPosition;

            if (distanceToPosition == 0)
            {
                return;
            }

            var positiveMovement = this.MotorConfiguration.ReferencingDirection == Direction.Left
                ? Direction.Right
                : Direction.Left;

            var negativeMovement = this.MotorConfiguration.ReferencingDirection == Direction.Left
                ? Direction.Left
                : Direction.Right;

            var direction = distanceToPosition > 0 ? positiveMovement : negativeMovement;
            distanceToPosition = Math.Abs(distanceToPosition);


            await this.StartMotorAndMoveDistanceAsync(Speed.Maximal, direction, (short) distanceToPosition, true);
        }

        /// <summary>
        /// References the <see cref="MotorPositionController"/> according to the <see cref="MotorPositionController.MotorConfiguration"/>.
        /// Moves the Motor in ReferencingDirection with ReferencingSpeed until ReferencingInput reaches the ReferencingInputState. 
        /// Then Motor moves in the opposite direction with ReferencingFinePositioningSpeed until ReferencingInput leaves ReferencingInputState. 
        /// After that <see cref="MotorPositionController.CurrentPosition"/> is zeroed.
        /// Position tracking is disabled during referencing. MotorPositionController has to be referenced before proper usage.
        /// </summary>
        /// <param name="timeout">Timeout for the referencing operation.</param>
        /// <returns>true, if successful; false, if unsuccessful</returns>
        public async Task<bool> MoveMotorToReferenceAsync(TimeSpan? timeout = null)
        {
            this.motorDistanceInfo.IsTracking = false;

            var freeRunDirection = MotorConfiguration.ReferencingDirection == Direction.Left
                ? Direction.Right
                : Direction.Left;

            if (controllerSequencer.GetDigitalInputState(MotorConfiguration.ReferencingInput) ==
                MotorConfiguration.ReferencingInputState)
            {
                var succeeded =
                    await
                        controllerSequencer.StartMotorStopWithDigitalInputInternalAsync(MotorConfiguration.Motor,
                            MotorConfiguration.ReferencingSpeed, freeRunDirection, MotorConfiguration.ReferencingInput,
                            !MotorConfiguration.ReferencingInputState, timeout);

                if (!succeeded)
                {
                    return false;
                }

                await
                    controllerSequencer.StartMotorStopAfterTimeSpanInternalAsync(MotorConfiguration.Motor,
                        MotorConfiguration.ReferencingSpeed, freeRunDirection, TimeSpan.FromMilliseconds(100));
            }

            var positionReached =
                await
                    controllerSequencer.StartMotorStopWithDigitalInputInternalAsync(MotorConfiguration.Motor,
                        MotorConfiguration.ReferencingSpeed, MotorConfiguration.ReferencingDirection,
                        MotorConfiguration.ReferencingInput, MotorConfiguration.ReferencingInputState, timeout);

            if (positionReached)
            {
                positionReached =
                    await
                        controllerSequencer.StartMotorStopWithDigitalInputInternalAsync(MotorConfiguration.Motor,
                            MotorConfiguration.ReferencingFinePositioningSpeed, freeRunDirection, MotorConfiguration.ReferencingInput,
                            !MotorConfiguration.ReferencingInputState, timeout);
            }

            Interlocked.Exchange(ref currentPosition, 0);
            this.OnPropertyChanged(nameof(CurrentPosition));

            await Task.Delay(TimeSpan.FromMilliseconds(500));

            this.motorDistanceInfo.IsTracking = true;

            return positionReached;
        }

        /// <summary>
        /// Delivers the current <see cref="MotorPositionInfo"/> of the controller.
        /// </summary>
        public MotorPositionInfo GetPositionInfo()
        {
            return new MotorPositionInfo
            {
                Motor = this.MotorConfiguration.Motor,
                Position = this.CurrentPosition
            };
        }

        public void Dispose()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int GetAvailableDistance(Direction direction)
        {
            if (direction != this.MotorConfiguration.ReferencingDirection)
            {
                return this.MotorConfiguration.Limit - this.CurrentPosition;
            }

            return this.MotorConfiguration.Limit;
        }
    }
}