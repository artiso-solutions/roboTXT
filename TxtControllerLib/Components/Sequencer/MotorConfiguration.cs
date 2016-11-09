using RoboticsTxt.Lib.Contracts;

namespace RoboticsTxt.Lib.Components.Sequencer
{
    public class MotorConfiguration
    {
        public MotorConfiguration()
        {
            Motor = Motor.Unknown;
            ReferencingInput = DigitalInput.Unknown;
            ReferencingDirection = Direction.Unknown;
            ReferencingSpeed = Speed.Off;
            ReferencingFinePositioningSpeed = Speed.Off;
            Limit = 0;

            this.IsSaveable = true;
        }

        /// <summary>
        /// Index of the motor to configure.
        /// </summary>
        /// <value>
        /// Possible values: <see cref="Contracts.Motor"/>
        /// </value>
        public Motor Motor { get; set; }

        /// <summary>
        /// Index of the digital input of the limit switch in <see cref="ReferencingDirection"/>.
        /// </summary>
        /// <value>
        /// Possible values: <see cref="DigitalInput"/>
        /// </value>
        public DigitalInput ReferencingInput { get; set; }

        /// <summary>
        /// Direction of motor movement toward <see cref="ReferencingInput"/>.
        /// </summary>
        /// <value>
        /// Possible values: <see cref="Direction"/>
        /// </value>
        public Direction ReferencingDirection { get; set; }

        /// <summary>
        /// Primary speed used for the referencing movement toward <see cref="ReferencingInput"/>.
        /// </summary>
        /// <value>
        /// Possible values: <see cref="Speed"/>
        /// </value>
        public Speed ReferencingSpeed { get; set; }

        /// <summary>
        /// Speed used for the secondary referencing movement away from <see cref="ReferencingInput"/>.
        /// </summary>
        /// <value>
        /// Possible values: <see cref="Speed"/>
        /// </value>
        public Speed ReferencingFinePositioningSpeed { get; set; }

        /// <summary>
        /// State of <see cref="ReferencingInput"/> when position is reached.
        /// </summary>
        public bool ReferencingInputState { get; set; }

        /// <summary>
        /// Soft-limit for the movement direction without limit switch.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Flag which determines whether the configured <see cref="MotorPositionController"/> is included in the saved positions of <see cref="ControllerSequencer.SaveCurrentPosition"/>.
        /// </summary>
        public bool IsSaveable { get; set; }
    }
}