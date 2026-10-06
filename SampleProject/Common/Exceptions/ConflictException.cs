using System;

namespace Common.Exceptions
{
    /// <summary>
    /// The operation conflicts with the current state of the data, e.g. creating an entity whose
    /// id is already taken or saving over a concurrent modification. Surfaces as HTTP 409.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }

        public ConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}