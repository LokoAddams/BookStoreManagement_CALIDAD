using System;
using System.Collections.Generic;

namespace ServiceCommon.Application.Services
{
    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ValidationException : Exception
    {
        public List<ValidationError> Errors { get; } = new();
        public ValidationException(IEnumerable<ValidationError> errors)
        {
            Errors.AddRange(errors);
        }
        public ValidationException(string message) : base(message) { }
    }
}
