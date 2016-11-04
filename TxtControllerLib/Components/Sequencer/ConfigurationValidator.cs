using System;
using JetBrains.Annotations;
using RoboticsTxt.Lib.Contracts;
using RoboticsTxt.Lib.Contracts.Exceptions;

namespace RoboticsTxt.Lib.Components.Sequencer
{
    internal static class ConfigurationValidator
    {
        public static void ValidateMotorConfiguration([NotNull] MotorConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (configuration.Motor == Motor.Unknown)
                throw new ConfigurationValidationException(nameof(MotorConfiguration), $"{nameof(configuration.Motor)} unknown.");

            if (configuration.ReferencingDirection == Direction.Unknown)
                throw new ConfigurationValidationException(nameof(MotorConfiguration), $"{nameof(configuration.ReferencingDirection)} unknown.");

            if (configuration.ReferencingInput == DigitalInput.Unknown)
                throw new ConfigurationValidationException(nameof(MotorConfiguration), $"{nameof(configuration.ReferencingInput)} unknown.");

            if (configuration.ReferencingSpeed == Speed.Off)
                throw new ConfigurationValidationException(nameof(MotorConfiguration), $"{nameof(configuration.ReferencingSpeed)} unknown.");

            if (configuration.ReferencingFinePositioningSpeed == Speed.Off)
                throw new ConfigurationValidationException(nameof(MotorConfiguration), $"{nameof(configuration.ReferencingFinePositioningSpeed)} unknown.");

            if (configuration.Limit <= 0)
                throw new ConfigurationValidationException(nameof(MotorConfiguration), $"{nameof(configuration.Limit)} is invalid.");
        }
    }
}
