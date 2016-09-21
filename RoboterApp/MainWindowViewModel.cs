using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JetBrains.Annotations;
using RoboterApp.Commands;
using RoboticsTxt.Lib.Commands;
using RoboticsTxt.Lib.Components;
using RoboticsTxt.Lib.Components.Sequencer;
using RoboticsTxt.Lib.Contracts;

namespace RoboterApp
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ControllerSequencer controllerSequencer;
        private string positionName;

        public MainWindowViewModel()
        {
            IPAddress ipAddress;

            if (!IPAddress.TryParse(Properties.Settings.Default.RoboAddress, out ipAddress))
            {
                var hostEntry = Dns.GetHostEntry(Properties.Settings.Default.RoboAddress);
                if (hostEntry.AddressList.Length != 1)
                {
                    throw new InvalidOperationException($"Did not find ip address for hostname {Properties.Settings.Default.RoboAddress}");
                }

                ipAddress = hostEntry.AddressList[0];
            }

            controllerSequencer = new ControllerSequencer(ipAddress);
            this.PositionNames = new ObservableCollection<string>(this.controllerSequencer.GetPositionNames());
            BackwardForwardPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.Two,
                ReferencingDirection = Direction.Left,
                ReferencingSpeed = Speed.Maximal,
                ReferencingInput = DigitalInput.Two,
                ReferencingInputState = false,
                Limit = 700
            });
            MoveBackwardCommand = new ContinuousMoveAxisCommand(BackwardForwardPositionController, Direction.Left, 100);
            MoveForwardCommand = new ContinuousMoveAxisCommand(BackwardForwardPositionController, Direction.Right, 100);

            UpDownPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.Three,
                ReferencingDirection = Direction.Left,
                ReferencingSpeed = Speed.Fast,
                ReferencingInput = DigitalInput.Three,
                ReferencingInputState = false,
                Limit = 2000
            });
            MoveUpCommand = new ContinuousMoveAxisCommand(UpDownPositionController, Direction.Left, 100);
            MoveDownCommand = new ContinuousMoveAxisCommand(UpDownPositionController, Direction.Right, 100);

            TurnLeftRightPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.One,
                ReferencingDirection = Direction.Right,
                ReferencingSpeed = Speed.Quick,
                ReferencingInput = DigitalInput.One,
                ReferencingInputState = false,
                Limit = 1500
            });
            TurnLeftCommand = new ContinuousMoveAxisCommand(TurnLeftRightPositionController, Direction.Left, 100);
            TurnRightCommand = new ContinuousMoveAxisCommand(TurnLeftRightPositionController, Direction.Right, 100);

            OpenCloseClampPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.Four,
                ReferencingDirection = Direction.Left,
                ReferencingSpeed = Speed.Quick,
                ReferencingInput = DigitalInput.Four,
                ReferencingInputState = false,
                Limit = 15
            });
            OpenClampCommand = new ContinuousMoveAxisCommand(OpenCloseClampPositionController, Direction.Left, 100);
            CloseClampCommand = new ContinuousMoveAxisCommand(OpenCloseClampPositionController, Direction.Right, 100);

            ReferenceAxisCommand = new ReferenceAxisCommand(TurnLeftRightPositionController, UpDownPositionController, BackwardForwardPositionController, OpenCloseClampPositionController);
            SavePositionCommand = new SavePositionCommand(this.controllerSequencer);
            MoveToPositionCommand = new MoveToPositionCommand(this.controllerSequencer);
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


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}