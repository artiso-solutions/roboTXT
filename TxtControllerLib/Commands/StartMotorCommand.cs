using System;
using JetBrains.Annotations;
using RoboticsTxt.Lib.Components.Communicator;
using RoboticsTxt.Lib.Contracts;
using RoboticsTxt.Lib.Interfaces;
using RoboticsTxt.Lib.Messages;

namespace RoboticsTxt.Lib.Commands
{
    internal class StartMotorCommand : IControllerCommand
    {
        private readonly short speed;
        private readonly Direction direction;

        public StartMotorCommand(Motor motor, short speed, Direction direction)
        {
            if (speed < 0 || speed > 512)
            {
                throw new ArgumentOutOfRangeException("Parameter \"speed\" is out of range. (min: 0, max: 512)");
            }
            this.Motor = motor;
            this.speed = speed;
            this.direction = direction;
        }

        public Motor Motor { get; }

        public void Execute(ControllerCommunicator controllerCommunicator, [NotNull] ExchangeDataCommandMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var motorIndex = (int)this.Motor;
            var speedValue = this.speed;

            switch (this.direction)
            {
                case Direction.Left:
                    message.PwmOutputValues[2 * motorIndex] = speedValue;
                    message.PwmOutputValues[2 * motorIndex + 1] = 0;
                    break;
                case Direction.Right:
                    message.PwmOutputValues[2 * motorIndex] = 0;
                    message.PwmOutputValues[2 * motorIndex + 1] = speedValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            message.MotorDistance[motorIndex] = 0;
        }
    }
}
