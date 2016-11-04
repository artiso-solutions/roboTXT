using RoboticsTxt.Lib.Contracts;

namespace RoboticsTxt.Lib.Components.Sequencer
{
    public class MotorConfiguration
    {
        public MotorConfiguration()
        {
            this.IsSaveable = true;
        }

        public Motor Motor { get; set; }

        public DigitalInput ReferencingInput { get; set; }

        public Direction ReferencingDirection { get; set; }

        public Speed ReferencingSpeed { get; set; }

        public Speed ReferencingFinePositioningSpeed { get; set; }

        public bool ReferencingInputState { get; set; }

        public int Limit { get; set; }

        public bool IsSaveable { get; set; }
    }
}