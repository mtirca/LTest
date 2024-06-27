using System;
using System.Collections.Generic;

namespace Exceptions
{
    public class InvalidChannelExpressionException : Exception
    {
        public List<string> Channels { get; }

        public InvalidChannelExpressionException(string message, List<string> invalidChannels)
            : base(message)
        {
            Channels = invalidChannels;
        }
    }
}