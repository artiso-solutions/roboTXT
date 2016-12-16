using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using JetBrains.Annotations;
using log4net;
using RoboticsTxt.Lib.Contracts.Exceptions;
using RoboticsTxt.Lib.Messages.Base;

namespace RoboticsTxt.Lib.ControllerDriver
{
    internal class TcpControllerDriver : IDisposable
    {
        private readonly ILog logger;
        private readonly IPAddress ipAddress;
        private Socket socket;

        public TcpControllerDriver(IPAddress ipAddress)
        {
            logger = LogManager.GetLogger(typeof(TcpControllerDriver));
            this.ipAddress = ipAddress;
        }

        public void Dispose()
        {
            if (this.socket?.Connected == true)
            {
                this.socket.Close();
                this.socket.Dispose();
                this.socket = null;
            }
        }

        public void StartCommunication()
        {
            this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(this.ipAddress, 65000);
        }

        public void SendCommand<TCmdMessage>([NotNull] TCmdMessage command)
            where TCmdMessage : CommandMessage
        {
            if (!this.socket.Connected)
            {
                logger.Debug("Restart socket connection as connection was not working anymore.");
                StartCommunication();
            }

            var cmdBytes = this.GetBytesOfMessage(command);
            this.socket.Send(cmdBytes);

            using (var receiveStream = new MemoryStream())
            {
                var receiveBuffer = new byte[256];
                int length;
                do
                {
                    length = this.socket.Receive(receiveBuffer);
                    receiveStream.Write(receiveBuffer, 0, length);
                } while (length == receiveBuffer.Length);

                var receivedBytes = receiveStream.ToArray();
                if (receivedBytes.Length <= 0)
                {
                    throw new CommunicationFailedException("Failed to receive response");
                }

                var responseMessageId = this.GetMessageId(receivedBytes);
                if (command.ExpectedResponseId != responseMessageId)
                {
                    throw new CommunicationFailedException(
                        $"Did not receive expected respone id {command.ExpectedResponseId}. Received response message of type {responseMessageId.GetType().FullName} instead.");
                }
            }
        }

        [NotNull]
        public TResponseMessage SendCommand<TCmdMessage, TResponseMessage>([NotNull] TCmdMessage command)
            where TCmdMessage : CommandMessage
            where TResponseMessage : ResponseMessage, new()
        {
            if (!this.socket.Connected)
            {
                logger.Debug("Restart socket connection as connection was not working anymore.");
                StartCommunication();
            }

            var cmdBytes = this.GetBytesOfMessage(command);
            this.socket.Send(cmdBytes);

            using (var receiveStream = new MemoryStream())
            {
                var receiveBuffer = new byte[256];
                int length;
                do
                {
                    length = this.socket.Receive(receiveBuffer);
                    receiveStream.Write(receiveBuffer, 0, length);
                } while (length == receiveBuffer.Length || receiveStream.Length == 0);

                var receviedBytes = receiveStream.ToArray();
                if (receviedBytes.Length <= 0)
                {
                    logger.Warn("Failed to receive response for message.");
                    throw new CommunicationFailedException("Failed to receive response");
                }

                var responseMessage = this.GetMessageOfBytes<TResponseMessage>(receviedBytes);
                if (command.ExpectedResponseId != responseMessage.CommandId)
                {
                    throw new CommunicationFailedException(
                        $"Did not receive expected respone id {command.ExpectedResponseId}. Received response message of type {responseMessage.GetType().FullName} instead.");
                }

                return (TResponseMessage)responseMessage;
            }
        }

        [NotNull]
        public byte[] GetBytesOfMessage([NotNull] CommandMessage message)
        {
            using (var memoryStream = new MemoryStream())
            {
                foreach (var propertySerializationInfo in message.SerializationProperties)
                {
                    propertySerializationInfo.WriteValue(memoryStream);
                }

                return memoryStream.ToArray();
            }
        }

        private uint GetMessageId([NotNull] byte[] bytes)
        {
            var startIndex = 0;
            var responseId = ArchiveReader.ReadUInt32(bytes, ref startIndex);

            return responseId;
        }

        [NotNull]
        private ControllerMessage GetMessageOfBytes<TResponseMessage>([NotNull] byte[] bytes)
            where TResponseMessage : ResponseMessage, new()
        {
            var responseMessage = new TResponseMessage();
            var deserializationContext = new DeserializationContext(bytes);

            foreach (var propertyDeserializationInfo in responseMessage.DeserializationProperties)
            {
                propertyDeserializationInfo.ReadValue(deserializationContext);
            }

            return responseMessage;
        }
    }
}
