using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JetBrains.Annotations;
using RoboterApp.Commands;
using RoboterApp.Components;
using RoboticsTxt.Lib.Components.Sequencer;
using RoboticsTxt.Lib.Contracts;
using RoboticsTxt.Lib.Contracts.Configuration;

namespace RoboterApp
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IControllerSequencer controllerSequencer;
        private readonly SequenceCommandLogic sequenceCommandLogic;

        private string positionName;
        private TimeSpan currentCommunicationLoopCycleTime;
        private Exception lastCommunicationLoopException;
        private bool currentControllerConnectionState;

        public MainWindowViewModel()
        {
            controllerSequencer = new ControllerSequencer(Properties.Settings.Default.RoboAddress, new ControllerConfiguration(), new ApplicationConfiguration() {ApplicationName = "RoboterApp"});
            this.PositionNames = new ObservableCollection<string>(this.controllerSequencer.GetPositionNames());

            BackwardForwardPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.Two,
                ReferencingDirection = Direction.Left,
                ReferencingSpeed = Speed.Maximal,
                ReferencingFinePositioningSpeed = Speed.Slow,
                ReferencingInput = DigitalInput.Two,
                ReferencingInputState = false,
                Limit = 700
            });
            MoveBackwardCommand = new ContinuousMoveAxisCommand(this.BackwardForwardPositionController, Direction.Left);
            MoveForwardCommand = new ContinuousMoveAxisCommand(this.BackwardForwardPositionController, Direction.Right);

            UpDownPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.Three,
                ReferencingDirection = Direction.Left,
                ReferencingSpeed = Speed.Fast,
                ReferencingFinePositioningSpeed = Speed.Slow,
                ReferencingInput = DigitalInput.Three,
                ReferencingInputState = false,
                Limit = 2000
            });
            MoveUpCommand = new ContinuousMoveAxisCommand(this.UpDownPositionController, Direction.Left);
            MoveDownCommand = new ContinuousMoveAxisCommand(this.UpDownPositionController, Direction.Right);

            TurnLeftRightPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.One,
                ReferencingDirection = Direction.Right,
                ReferencingSpeed = Speed.Average,
                ReferencingFinePositioningSpeed = Speed.Slow,
                ReferencingInput = DigitalInput.One,
                ReferencingInputState = false,
                Limit = 2200
            });
            TurnLeftCommand = new ContinuousMoveAxisCommand(this.TurnLeftRightPositionController, Direction.Left);
            TurnRightCommand = new ContinuousMoveAxisCommand(this.TurnLeftRightPositionController, Direction.Right);

            OpenCloseClampPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.Four,
                ReferencingDirection = Direction.Left,
                ReferencingSpeed = Speed.Quick,
                ReferencingFinePositioningSpeed = Speed.Slow,
                ReferencingInput = DigitalInput.Four,
                ReferencingInputState = false,
                Limit = 15,
                IsSaveable = false
            });
            OpenClampCommand = new ContinuousMoveAxisCommand(this.OpenCloseClampPositionController, Direction.Left);
            CloseClampCommand = new ContinuousMoveAxisCommand(this.OpenCloseClampPositionController, Direction.Right);

            ReferenceAxisCommand = new ReferenceAxisCommand(TurnLeftRightPositionController, UpDownPositionController, BackwardForwardPositionController, OpenCloseClampPositionController);
            SavePositionCommand = new SavePositionCommand(this.controllerSequencer, this.PositionNames);
            MoveToPositionCommand = new MoveToPositionCommand(this.controllerSequencer, this);

            this.sequenceCommandLogic = new SequenceCommandLogic(this.controllerSequencer, this.BackwardForwardPositionController, this.UpDownPositionController, this.TurnLeftRightPositionController, this.OpenCloseClampPositionController);
            StartSequenceCommand = new StartSequenceCommand(this.sequenceCommandLogic);

            AlarmSoundCommand = new AlarmSoundCommand(controllerSequencer);

            controllerSequencer.CommunicationLoopCyleTimeChanges.Subscribe(OnCommunicationLoopCycleTimeUpdate);
            controllerSequencer.CommunicationExceptions.Subscribe(OnCommunicationLoopExcpetion);
            controllerSequencer.ControllerConnectionStateChanges.Subscribe(OnControllerConnectionStateChanged);
            CurrentControllerConnectionState = controllerSequencer.CurrentlyConnectedToController;
        }

        private void OnControllerConnectionStateChanged(bool newConnectionState)
        {
            CurrentControllerConnectionState = newConnectionState;
        }

        private void OnCommunicationLoopExcpetion(Exception exception)
        {
            LastCommunicationLoopException = exception;
        }

        private void OnCommunicationLoopCycleTimeUpdate(TimeSpan timeSpan)
        {
            CurrentCommunicationLoopCycleTime = timeSpan;
        }

        public MotorPositionController OpenCloseClampPositionController { get; }

        public MotorPositionController TurnLeftRightPositionController { get; }

        public MotorPositionController UpDownPositionController { get; }

        public MotorPositionController BackwardForwardPositionController { get; }

        public void Dispose()
        {
            controllerSequencer.Dispose();
        }

        public ICommand MoveToPositionCommand { get; set; }
        public ICommand ReferenceAxisCommand { get; }
        public ICommand SavePositionCommand { get; set; }
        public ICommand StartSequenceCommand { get; }
        public ICommand AlarmSoundCommand { get; }

        public ContinuousMoveAxisCommand MoveBackwardCommand { get; }
        public ContinuousMoveAxisCommand MoveForwardCommand { get; }
        public ContinuousMoveAxisCommand MoveUpCommand { get; }
        public ContinuousMoveAxisCommand MoveDownCommand { get; }
        public ContinuousMoveAxisCommand TurnLeftCommand { get; }
        public ContinuousMoveAxisCommand TurnRightCommand { get; }
        public ContinuousMoveAxisCommand OpenClampCommand { get; }
        public ContinuousMoveAxisCommand CloseClampCommand { get; }

        public string PositionName
        {
            get { return this.positionName; }
            set
            {
                this.positionName = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<string> PositionNames { get; }

        public TimeSpan CurrentCommunicationLoopCycleTime
        {
            get { return currentCommunicationLoopCycleTime; }
            private set
            {
                if (value == currentCommunicationLoopCycleTime) return;
                currentCommunicationLoopCycleTime = value;
                OnPropertyChanged();
            }
        }

        public Exception LastCommunicationLoopException
        {
            get { return lastCommunicationLoopException; }
            private set
            {
                if (value == lastCommunicationLoopException) return;
                lastCommunicationLoopException = value;
                OnPropertyChanged();
            }
        }

        public bool CurrentControllerConnectionState
        {
            get { return currentControllerConnectionState; }
            private set
            {
                if (value == currentControllerConnectionState) return;
                currentControllerConnectionState = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}