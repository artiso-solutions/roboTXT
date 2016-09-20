using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JetBrains.Annotations;
using RoboterApp.Commands;
using RoboticsTxt.Lib.Components;
using RoboticsTxt.Lib.Contracts;

namespace RoboterApp
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ControllerSequencer controllerSequencer;

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

            var backwardForwardPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration {
                Motor = Motor.Two,
                ReferencingMovement = Movement.Left,
                ReferencingSpeed = Speed.Maximal,
                ReferencingInput = DigitalInput.Two,
                ReferencingInputState = false
            });
            MoveBackwardCommand = new ContinuousMoveAxisCommand(backwardForwardPositionController, Movement.Left, 100);
            MoveForwardCommand = new ContinuousMoveAxisCommand(backwardForwardPositionController, Movement.Right, 100);

            var upDownPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.Three,
                ReferencingMovement = Movement.Left,
                ReferencingSpeed = Speed.Fast,
                ReferencingInput = DigitalInput.Three,
                ReferencingInputState = false
            });
            MoveUpCommand = new ContinuousMoveAxisCommand(upDownPositionController, Movement.Left, 100);
            MoveDownCommand = new ContinuousMoveAxisCommand(upDownPositionController, Movement.Right, 100);

            var turnLeftRightPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.One,
                ReferencingMovement = Movement.Right,
                ReferencingSpeed = Speed.Quick,
                ReferencingInput = DigitalInput.One,
                ReferencingInputState = false
            });
            TurnLeftCommand = new ContinuousMoveAxisCommand(turnLeftRightPositionController, Movement.Left, 100);
            TurnRightCommand = new ContinuousMoveAxisCommand(turnLeftRightPositionController, Movement.Right, 100);

            var openCloseClampPositionController = controllerSequencer.ConfigureMotorPositionController(new MotorConfiguration
            {
                Motor = Motor.Four,
                ReferencingMovement = Movement.Left,
                ReferencingSpeed = Speed.Quick,
                ReferencingInput = DigitalInput.Four,
                ReferencingInputState = false
            });
            OpenClampCommand = new ContinuousMoveAxisCommand(openCloseClampPositionController, Movement.Left, 100);
            CloseClampCommand = new ContinuousMoveAxisCommand(openCloseClampPositionController, Movement.Right, 100);
            
            ReferenceAxisCommand = new ReferenceAxisCommand(turnLeftRightPositionController, upDownPositionController, backwardForwardPositionController, openCloseClampPositionController);
        }

        public void Dispose()
        {
            controllerSequencer.Dispose();
        }

        public ICommand ReferenceAxisCommand { get; }
        public ContinuousMoveAxisCommand MoveBackwardCommand { get; }
        public ContinuousMoveAxisCommand MoveForwardCommand { get; }
        public ContinuousMoveAxisCommand MoveUpCommand { get; }
        public ContinuousMoveAxisCommand MoveDownCommand { get; }
        public ContinuousMoveAxisCommand TurnLeftCommand { get; }
        public ContinuousMoveAxisCommand TurnRightCommand { get; }
        public ContinuousMoveAxisCommand OpenClampCommand { get; }
        public ContinuousMoveAxisCommand CloseClampCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}