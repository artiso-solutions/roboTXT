using System.Collections.Generic;
using RoboticsTxt.Lib.Contracts.Exceptions;

namespace RoboticsTxt.Lib.Messages.Base
{
    internal class ResponseMessage : ControllerMessage
    {
        public ResponseMessage(uint commandId)
        {
            this.DeserializationProperties = new List<PropertyDeserializationInfo>();
            this.AddProperty("CommandId", dc =>
            {
                var responseCommandId = ArchiveReader.ReadUInt32(dc);
                if (responseCommandId != commandId)
                {
                    throw new CommunicationFailedException(
                        $"Did not receive expected respone id {commandId}. Received response message id {responseCommandId} instead.");
                }

                this.CommandId = responseCommandId;
            });
        }

        public List<PropertyDeserializationInfo> DeserializationProperties { get; }
    }
}