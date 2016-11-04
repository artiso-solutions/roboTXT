using System;

namespace RoboticsTxt.Lib.Contracts.Exceptions
{
    public class ConfigurationValidationException : Exception
    {
        public ConfigurationValidationException(string configurationType, string invalidationReason) : base($"Invalid configuration of type: {configurationType}\nReason: {invalidationReason}")
        {
        }
    }
}