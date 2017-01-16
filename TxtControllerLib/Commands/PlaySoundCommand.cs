using System;
using JetBrains.Annotations;
using RoboticsTxt.Lib.Components.Communicator;
using RoboticsTxt.Lib.Contracts;
using RoboticsTxt.Lib.Interfaces;
using RoboticsTxt.Lib.Messages;

namespace RoboticsTxt.Lib.Commands
{
    internal class PlaySoundCommand : IControllerCommand
    {
        private readonly Sound sound;
        private readonly ushort repetitions;

        public PlaySoundCommand(Sound sound, ushort repetitions)
        {
            this.sound = sound;
            this.repetitions = repetitions;
        }

        public void Execute(ControllerCommunicator controllerCommunicator, [NotNull] ExchangeDataCommandMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.SoundIndex = (ushort) sound;
            message.SoundRepeat = repetitions;
            message.SoundCommandId++;
        }
    }
}
