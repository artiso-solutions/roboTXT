﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RoboticsTxt.Lib.Commands;
using RoboticsTxt.Lib.Components.Communicator;
using RoboticsTxt.Lib.Contracts;

namespace RoboticsTxt.Lib.Components.Sequencer
{
    /// <summary>
    /// The <see cref="ControllerSequencer"/> provides high level methods for operation of the Fischertechnik ROBOTICS TXT controller.
    /// This includes the operation of motors and inputs of any kind.
    /// </summary>
    /// <remarks>
    /// The operations provided by the <see cref="ControllerSequencer"/> are not implemented completely. More operations will follow...
    /// </remarks>
    public class ControllerSequencer : IDisposable
    {
        private readonly ControllerCommunicator controllerCommunicator;
        private readonly Dictionary<Motor, MotorPositionController> motorPositionControllers;

        private List<Position> positions; 

        /// <summary>
        /// Creates a new instance of the <see cref="ControllerSequencer"/> and starts the communication with the controller. To stop the communication
        /// you have to dispose the <see cref="ControllerSequencer"/>.
        /// </summary>
        /// <param name="ipAddress"></param>
        public ControllerSequencer(IPAddress ipAddress)
        {
            this.controllerCommunicator = new ControllerCommunicator(ipAddress);
            this.motorPositionControllers = new Dictionary<Motor, MotorPositionController>();

            this.positions = new List<Position>();
            this.LoadPositionsFromFile();

            this.controllerCommunicator.Start();
        }

        /// <summary>
        /// Starts the specified <paramref name="motor"/> immediately.
        /// </summary>
        /// <param name="motor">The motor to start.</param>
        /// <param name="speed">The speed of the motor.</param>
        /// <param name="movement">The direction to start.</param>
        public void StartMotor(Motor motor, Speed speed, Movement movement)
        {
            CheckMotorPositionMode(motor);

            this.controllerCommunicator.QueueCommand(new StartMotorCommand(motor, speed, movement));
        }

        /// <summary>
        /// Stops the specified <paramref name="motor"/> immediately.
        /// </summary>
        /// <param name="motor">The motor to stop.</param>
        public void StopMotor(Motor motor)
        {
            CheckMotorPositionMode(motor);

            this.controllerCommunicator.QueueCommand(new StopMotorCommand(motor));
        }

        /// <summary>
        /// Starts the specified <paramref name="motor"/> and stops it on state trigger of the specified <paramref name="digitalInput"/>.
        /// </summary>
        /// <param name="motor">The motor to start.</param>
        /// <param name="speed">The speed of the motor.</param>
        /// <param name="movement">The direction to start.</param>
        /// <param name="digitalInput">The digital input to trigger the stop.</param>
        /// <param name="expectedInputState">The expected value for the state trigger.</param>
        /// <returns>This method is async. The returned task will be completed as soon as the movement is finished.</returns>
        public async Task StartMotorStopWithDigitalInputAsync(Motor motor, Speed speed, Movement movement, DigitalInput digitalInput, bool expectedInputState)
        {
            CheckMotorPositionMode(motor);

            await StartMotorStopWithDigitalInputInternalAsync(motor, speed, movement, digitalInput, expectedInputState);
        }

        internal async Task StartMotorStopWithDigitalInputInternalAsync(Motor motor, Speed speed, Movement movement, DigitalInput digitalInput, bool expectedInputState)
        {
            this.controllerCommunicator.QueueCommand(new StartMotorCommand(motor, speed, movement));
            await this.WaitForInputAsync(digitalInput, expectedInputState);
            this.controllerCommunicator.QueueCommand(new StopMotorCommand(motor));
        }

        /// <summary>
        /// Starts the specified <paramref name="motor"/> and stops it after the given time span <paramref name="stopAfterTimeSpan"/>.
        /// </summary>
        /// <param name="motor">The motor to start.</param>
        /// <param name="speed">The speed of the motor.</param>
        /// <param name="movement">The direction to start.</param>
        /// <param name="stopAfterTimeSpan">The time span which is used to stop the motor again.</param>
        /// <returns>This method is async. The returned task will be completed as soon as the movement is finished.</returns>
        public async Task StartMotorStopAfterTimeSpanAsync(Motor motor, Speed speed, Movement movement, TimeSpan stopAfterTimeSpan)
        {
            CheckMotorPositionMode(motor);

            await StartMotorStopAfterTimeSpanInternalAsync(motor, speed, movement, stopAfterTimeSpan);
        }

        internal async Task StartMotorStopAfterTimeSpanInternalAsync(Motor motor, Speed speed, Movement movement, TimeSpan stopAfterTimeSpan)
        {
            this.controllerCommunicator.QueueCommand(new StartMotorCommand(motor, speed, movement));
            await Task.Delay(stopAfterTimeSpan);
            this.controllerCommunicator.QueueCommand(new StopMotorCommand(motor));
        }

        /// <summary>
        /// Retrieves the current input state of the specified <paramref name="referenceInput"/>.
        /// </summary>
        /// <param name="referenceInput">The digital input to get the state from.</param>
        /// <returns><c>true</c> if the input is triggered, otherwise <c>false</c>.</returns>
        public bool GetDigitalInputState(DigitalInput referenceInput)
        {
            return this.controllerCommunicator.UniversalInputs[(int)referenceInput].CurrentState;
        }

        public void SaveCurrentPosition(string positionName)
        {
            var newPosition = new Position {PositionName = positionName};

            foreach (var motorPositionController in this.motorPositionControllers.Values)
            {
                newPosition.MotorPositionInfos.Add(motorPositionController.GetPositionInfo());
            }

            this.positions.Add(newPosition);

            this.SavePositionsToFile();
        }

        private void SavePositionsToFile()
        {
            try
            {
                var positionsJson = JsonConvert.SerializeObject(this.positions);

                var stream = new FileStream("PositionFile.json", FileMode.Create);
                var streamWriter = new StreamWriter(stream);

                streamWriter.Write(positionsJson);
                streamWriter.Flush();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void LoadPositionsFromFile()
        {
            try
            {
                var stream = new FileStream("PositionFile.json", FileMode.Open);
                var streamReader = new StreamReader(stream);

                var positionsJson = streamReader.ReadToEnd();

                this.positions = JsonConvert.DeserializeObject<List<Position>>(positionsJson);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// Cleanup of all resrouces. This also stops the communication to the controller.
        /// </summary>
        public void Dispose()
        {
            foreach (var motorPositionController in motorPositionControllers)
            {
                motorPositionController.Value.Dispose();
            }

            this.controllerCommunicator.Stop();
        }

        public MotorPositionController ConfigureMotorPositionController(MotorConfiguration motorConfiguration)
        {
            MotorPositionController configureMotorPositionController;
            if (motorPositionControllers.TryGetValue(motorConfiguration.Motor, out configureMotorPositionController))
            {
                return configureMotorPositionController;
            }

            configureMotorPositionController = new MotorPositionController(motorConfiguration, controllerCommunicator, this);
            motorPositionControllers[motorConfiguration.Motor] = configureMotorPositionController;
            return configureMotorPositionController;
        }

        public void ReleaseMotorPositionController(MotorPositionController motorPositionController)
        {
            motorPositionControllers.Remove(motorPositionController.MotorConfiguration.Motor);
            motorPositionController.Dispose();
        }

        private async Task WaitForInputAsync(DigitalInput digitalInput, bool expectedValue)
        {
            await this.controllerCommunicator.UniversalInputs[(int)digitalInput].StateChanges.FirstAsync(b => b == expectedValue);
        }

        private void CheckMotorPositionMode(Motor motor)
        {
            if (motorPositionControllers.ContainsKey(motor))
            {
                throw new InvalidOperationException($"Motor {motor} is configured for position control and can not be commanded via controller sequencer. Use the MotorPositionController.");
            }
        }
    }
}