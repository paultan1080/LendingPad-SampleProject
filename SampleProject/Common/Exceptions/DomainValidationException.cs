using System;

namespace Common.Exceptions
{
    /// <summary>
    /// A business rule rejected a value, e.g. a required field is missing or out of range.
    /// Surfaces as HTTP 400, reported against <see cref="MemberName"/> like request validation errors.
    /// </summary>
    public class DomainValidationException : Exception
    {
        public DomainValidationException(string memberName, string message) : base(message)
        {
            MemberName = memberName;
        }

        public string MemberName { get; }
    }
}