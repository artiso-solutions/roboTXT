using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboticsTxt.Lib.Components.Sequencer;
using RoboticsTxt.Lib.Contracts;
using RoboticsTxt.Lib.Contracts.Exceptions;

namespace TxtControllerLibTests
{
    [TestClass]
    public class ConfigurationValidatorTests
    {
        [TestMethod]
        public void UninitalizedMotorConfiguration()
        {
            var configuration = new MotorConfiguration();

            var action = new Action(() => ConfigurationValidator.ValidateMotorConfiguration(configuration));

            action.ShouldThrow<ConfigurationValidationException>("configuration is not initialized");
        }

        [TestMethod]
        public void InvalidMotorConfigurations()
        {
            var configurationWithoutMotor = new MotorConfiguration
            {
                ReferencingDirection = Direction.Left,
                ReferencingInput = DigitalInput.One,
                ReferencingSpeed = Speed.Fast,
                ReferencingFinePositioningSpeed = Speed.Slow,
                Limit = 42
            };

            var configurationWithoutDirection = new MotorConfiguration
            {
                Motor = Motor.One,
                ReferencingInput = DigitalInput.One,
                ReferencingSpeed = Speed.Fast,
                ReferencingFinePositioningSpeed = Speed.Slow,
                Limit = 42
            };

            var configurationWithoutInput = new MotorConfiguration
            {
                Motor = Motor.One,
                ReferencingDirection = Direction.Left,
                ReferencingSpeed = Speed.Fast,
                ReferencingFinePositioningSpeed = Speed.Slow,
                Limit = 42
            };

            var configurationWithoutSpeed = new MotorConfiguration
            {
                Motor = Motor.One,
                ReferencingDirection = Direction.Left,
                ReferencingInput = DigitalInput.One,
                ReferencingFinePositioningSpeed = Speed.Slow,
                Limit = 42
            };

            var configurationWithoutFinePositioningSpeed = new MotorConfiguration
            {
                Motor = Motor.One,
                ReferencingDirection = Direction.Left,
                ReferencingInput = DigitalInput.One,
                ReferencingSpeed = Speed.Fast,
                Limit = 42
            };

            var configurationWithoutLimit = new MotorConfiguration
            {
                Motor = Motor.One,
                ReferencingDirection = Direction.Left,
                ReferencingInput = DigitalInput.One,
                ReferencingSpeed = Speed.Fast,
                ReferencingFinePositioningSpeed = Speed.Slow
            };

            var action = new Action(() => ConfigurationValidator.ValidateMotorConfiguration(configurationWithoutMotor));
            action.ShouldThrow<ConfigurationValidationException>("Motor is invalid");

            action = new Action(() => ConfigurationValidator.ValidateMotorConfiguration(configurationWithoutDirection));
            action.ShouldThrow<ConfigurationValidationException>("ReferencingDirection is invalid");

            action = new Action(() => ConfigurationValidator.ValidateMotorConfiguration(configurationWithoutInput));
            action.ShouldThrow<ConfigurationValidationException>("ReferencingInput is invalid");

            action = new Action(() => ConfigurationValidator.ValidateMotorConfiguration(configurationWithoutSpeed));
            action.ShouldThrow<ConfigurationValidationException>("ReferencingSpeed is invalid");

            action = new Action(() => ConfigurationValidator.ValidateMotorConfiguration(configurationWithoutFinePositioningSpeed));
            action.ShouldThrow<ConfigurationValidationException>("ReferencingFinePositioningSpeed is invalid");

            action = new Action(() => ConfigurationValidator.ValidateMotorConfiguration(configurationWithoutLimit));
            action.ShouldThrow<ConfigurationValidationException>("Limit is invalid");
        }

        [TestMethod]
        public void ValidMotorConfiguration()
        {
            var configuration = new MotorConfiguration
            {
                Motor = Motor.One,
                ReferencingDirection = Direction.Left,
                ReferencingInput = DigitalInput.One,
                ReferencingSpeed = Speed.Fast,
                ReferencingFinePositioningSpeed = Speed.Slow,
                Limit = 42
            };

            var action = new Action(() => ConfigurationValidator.ValidateMotorConfiguration(configuration));

            action.ShouldNotThrow("configuration is valid");
        }
    }
}
