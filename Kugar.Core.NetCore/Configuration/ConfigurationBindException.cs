using System;

namespace Kugar.Core.Configuration;

public class ConfigurationBindException : Exception
{
    public ConfigurationBindException(string message) : base(message)
    {
    }
}