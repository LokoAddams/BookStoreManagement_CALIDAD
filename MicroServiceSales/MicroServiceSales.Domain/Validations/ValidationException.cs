using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroServiceSales.Domain.Validations
{
    public class ValidationException : Exception
    {
        public IReadOnlyList<ValidationError> Errors { get; }

        public ValidationException(IEnumerable<ValidationError> errors)
            : base("Validation failed")
        {
            Errors = errors.ToList();
        }

        public ValidationException(params ValidationError[] errors)
            : base("Validation failed")
        {
            Errors = errors;
        }
    }
}
