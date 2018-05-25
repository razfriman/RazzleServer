using System;

namespace RazzleServer.Common.Exceptions
{
    public class NoAccountException : Exception
    {
        public override string Message => "The specified account does not exist.";
    }
}
