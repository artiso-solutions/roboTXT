﻿using System;
using JetBrains.Annotations;
using RoboticsTxt.Lib.Components.Communicator;
using RoboticsTxt.Lib.Contracts;
using RoboticsTxt.Lib.Interfaces;
using RoboticsTxt.Lib.Messages;

namespace RoboticsTxt.Lib.Commands
{
    internal class StartMotorCommand : IControllerCommand
    {
        private readonly Speed speed;
        private readonly Direction direction;

        public StartMotorCommand(Motor motor, Speed speed, Direction direction)
        {
            this.Motor = motor;
            this.speed = speed;
            this.direction = direction;
        }

        public Motor Motor { get; }

        public void Execute(ControllerCommunicator controllerCommunicator, [NotNull] ExchangeDataCommandMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var motorIndex = (int)this.Motor;
            var speedValue = (short)this.speed;

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

    internal class SetMotorOutputCommand : IControllerCommand
    {
        private readonly Speed speedO1;
        private readonly Speed speedO2;

        public SetMotorOutputCommand(Motor motor, Speed speedO1, Speed speedO2)
        {
            this.speedO1 = speedO1;
            this.speedO2 = speedO2;
            this.Motor = motor;
        }

        public Motor Motor { get; }

        public void Execute(ControllerCommunicator controllerCommunicator, [NotNull] ExchangeDataCommandMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var motorIndex = (int)this.Motor;
            message.PwmOutputValues[2 * motorIndex] = (short)speedO1;
            message.PwmOutputValues[2 * motorIndex + 1] = (short)speedO2;
            
            message.MotorDistance[motorIndex] = 0;
        }
    }
}
