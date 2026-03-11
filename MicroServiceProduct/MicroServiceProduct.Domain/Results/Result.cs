// New file: Result and Result<T>
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroServiceProduct.Domain.Results
{
    public class Result
    {
        public bool IsSuccess { get; }
        public IReadOnlyCollection<MicroServiceProduct.Domain.Validations.ValidationError>? Errors { get; }

        protected Result(bool isSuccess, IReadOnlyCollection<MicroServiceProduct.Domain.Validations.ValidationError>? errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static Result FromValidation(IEnumerable<MicroServiceProduct.Domain.Validations.ValidationError> errors)
        {
            var list = (errors ?? Enumerable.Empty<MicroServiceProduct.Domain.Validations.ValidationError>()).ToList();
            return list.Count == 0 ? new Result(true, null) : new Result(false, list);
        }
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        protected Result(bool isSuccess, T? value, IReadOnlyCollection<MicroServiceProduct.Domain.Validations.ValidationError>? errors)
            : base(isSuccess, errors)
        {
            Value = value;
        }

        public static Result<T> Ok(T value) => new Result<T>(true, value, null);

        public static Result<T> FromErrors(IEnumerable<MicroServiceProduct.Domain.Validations.ValidationError> errors)
        {
            var list = (errors ?? Enumerable.Empty<MicroServiceProduct.Domain.Validations.ValidationError>()).ToList();
            return new Result<T>(false, default, list);
        }
    }
}

