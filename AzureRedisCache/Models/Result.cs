using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AzureRedisCache.Models
{
    public class Result<T>
    {
        public T Value { get; }
        public bool Issuccess { get; }
        public string Error { get; }

        protected Result(T value, bool success, string error) 
        {   
            Value = value;
            Issuccess = success;
            Error = error;
        
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, string.Empty);
        public static Result<T> Failure(string error) => new Result<T>(default, false, error);
    }
}
