using System;

namespace JBA.Exceptions
{
    internal class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}