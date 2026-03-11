using MicroServiceDistributors.Domain.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceDistributors.Application.Services
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
