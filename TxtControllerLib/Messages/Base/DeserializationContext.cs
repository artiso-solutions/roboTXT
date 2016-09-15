namespace RoboticsTxt.Lib.Messages.Base
{
    public class DeserializationContext
    {
        public DeserializationContext(byte[] buffer)
        {
            this.Buffer = buffer;
            this.CurrentPosition = 0;
        }

        public byte[] Buffer { get; }

        public int CurrentPosition { get; set; }
    }
}