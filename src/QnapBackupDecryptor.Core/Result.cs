using System;

namespace QnapBackupDecryptor.Core
{
    public class Result<T>
    {
        public Exception Exception { get; }
        public bool IsSuccess { get; }
        public bool IsError => !IsSuccess;
        public string ErrorMessage { get; }
        public string SuccessMessage { get; }

        public T Data { get; }

        internal Result(bool isSuccess, string errorMessage, T data, string successMessage, Exception exception)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Data = data;
            SuccessMessage = successMessage;
            Exception = exception;
        }

        public static Result<T> OkResult(T data, string message)
            => new Result<T>(true, string.Empty, data, message, null);

        public static Result<T> OkResult(T data)
            => new Result<T>(true, string.Empty, data, string.Empty, null);

        public static Result<T> ErrorResult(string errorMessage, T data)
            => new Result<T>(false, errorMessage, data, string.Empty, null);

        public static Result<T> ErrorResult<TE>(string errorMessage, T data, TE exception) where TE : Exception
            => new Result<T>(false, errorMessage, data, string.Empty, exception);

        public static implicit operator T(Result<T> a)
            => a.Data;
    }

}
