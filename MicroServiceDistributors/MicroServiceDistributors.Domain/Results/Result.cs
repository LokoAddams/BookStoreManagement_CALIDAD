using MicroServiceDistributors.Domain.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceDistributors.Domain.Results
{
    public readonly struct Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public IReadOnlyList<ValidationError> Errors { get; }

        private Result(bool isSuccess, IReadOnlyList<ValidationError> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors ?? Array.Empty<ValidationError>();
        }

        public static Result Ok() => new(true, Array.Empty<ValidationError>());

        public static Result Fail(params ValidationError[] errors)
            => new(false, (errors ?? Array.Empty<ValidationError>()).ToArray());

        public static Result FromErrors(IEnumerable<ValidationError>? errors)
        {
            var list = (errors ?? Enumerable.Empty<ValidationError>()).ToList();
            return list.Count == 0 ? Ok() : new Result(false, list);
        }

        public static Result FromValidation(IEnumerable<ValidationError> validation)
            => FromErrors(validation);

        public static Result Combine(params Result[] results)
        {
            if (results == null || results.Length == 0) return Ok();
            var failed = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
            return failed.Count == 0 ? Ok() : new Result(false, failed);
        }

        public override string ToString()
        {
            if (IsSuccess) return "Ok";
            if (Errors.Count == 0) return "Fail";
            return $"Fail: {string.Join(" | ", Errors.Select(e => e.ToString()))}";
        }
    }

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

        public static Result<T> FromErrors(IEnumerable<ValidationError>? errors)
        {
            var list = (errors ?? Enumerable.Empty<ValidationError>()).ToList();
            return list.Count == 0
                ? new Result<T>(true, default!, Array.Empty<ValidationError>())
                : new Result<T>(false, default, list);
        }

        public static Result<T> FromValidation(IEnumerable<ValidationError> validation)
            => FromErrors(validation);

        public Result WithoutValue() => IsSuccess ? Result.Ok() : Result.FromErrors(Errors);

        public override string ToString()
        {
            if (IsSuccess) return Value?.ToString() ?? "Ok";
            if (Errors.Count == 0) return "Fail";
            return $"Fail: {string.Join(" | ", Errors.Select(e => e.ToString()))}";
        }
    }
}
