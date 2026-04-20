using MicroServiceUsers.Domain.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceUsers.Domain.Results
{
    

    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; }
        public IReadOnlyList<ValidationError> Errors { get; }

        private Result(bool isSuccess, T? value, IReadOnlyList<ValidationError> errors)
        {
            IsSuccess = isSuccess;
            Value = value;
            Errors = errors ?? Array.Empty<ValidationError>();
        }

        public static Result<T> Ok(T value) => new(true, value, Array.Empty<ValidationError>());

        public static Result<T> Fail(params ValidationError[] errors)
            => new(false, default, (errors ?? Array.Empty<ValidationError>()).ToArray());
    }
}
