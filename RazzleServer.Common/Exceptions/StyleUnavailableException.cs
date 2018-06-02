using System;

namespace RazzleServer.Common.Exceptions
{
    public class StyleUnavailableException : Exception
    {
        public override string Message => "The specified style does not exist.";
    }
}
