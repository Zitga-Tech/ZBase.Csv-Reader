using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CsvReader
{
    public interface IConvert<in TFrom, out TTo>
    {
        TTo Convert(object value);
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ConverterAttribute : Attribute
    {
        public Type Type { get; }

        public Type FromType { get; }

#if UNITY_EDITOR
        public int CallerLineNumber { get; }

        public string CallerFilePath { get; }
#endif

        public ConverterAttribute([NotNull] Type type, Type fromType = null
#if UNITY_EDITOR
            , [CallerLineNumber] int callerLineNumber = 0
            , [CallerFilePath] string callerFilePath = ""
#endif
        )
        {
            Type = type;
            FromType = fromType;

#if UNITY_EDITOR
            CallerLineNumber = callerLineNumber;
            CallerFilePath = callerFilePath;
#endif
        }
    }
}
