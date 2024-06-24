#if UNITY_EDITOR && ODIN_INSPECTOR

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

namespace CsvReader
{
    public static class CsvReaderUtils
    {
        private static readonly HashSet<Type> s_numericTypes = new() {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),  
            typeof(uint), typeof(float)
        };
        
        public static Type GetElementTypeFromFieldInfo(FieldInfo tmp)
        {
            string fullName = string.Empty;
            if (tmp.FieldType.IsArray)
            {
                if (tmp.FieldType.FullName != null)
                    fullName = tmp.FieldType.FullName.Substring(0, tmp.FieldType.FullName.Length - 2);
            }
            else
            {
                fullName = tmp.FieldType.FullName;
            }

            return GetType(fullName);
        }
        
        public static Type GetElementTypeFromFieldInfo(Type type)
        {
            string fullName = string.Empty;

            if (type.IsArray)
            {
                if (type.FullName != null)
                    fullName = type.FullName.Substring(0, type.FullName.Length - 2);
            }
            else
            {
                fullName = type.FullName;
            }

            return GetType(fullName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="strFullyQualifiedName"></param>
        /// <returns></returns>
        public static Type GetType(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type == null)
            {
                foreach (var asm in CsvConfig.Instance.ScriptAssemblies)
                {
                    type = asm.GetType(strFullyQualifiedName);
                    if (type != null)
                        break;
                }
            }

            if (type == null)
            {
                throw new Exception("Type is null: " + strFullyQualifiedName);
            }

            return type;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsValidKeyFormat(string key)
        {
            return key.Equals(key.ToLower());
        }

        /// <summary>
        /// Use to check variable and array variables is Primitive or not.
        /// Can't use IsClass or IsPrimitive because Array is always a class.
        /// Want to check the real type of element in array
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isCustomPrimitiveArray"></param>
        /// <returns></returns>
        public static bool IsPrimitive(Type type, bool isCustomPrimitiveArray)
        {
            return IsPrimitive(type.IsArray && isCustomPrimitiveArray ? GetElementTypeFromFieldInfo(type) : type);
        }

        public static bool IsPrimitive(Type type)
        {
            return type == typeof(string) || type.IsEnum || type.IsPrimitive;
        }
    
        public static bool IsNumeric(this Type myType)
        {
            return s_numericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);
        }

        public static string ConvertSnakeCaseToCamelCase(string snakeCase)
        {
            var strings = snakeCase.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            var result = strings[0];
            for (int i = 1; i < strings.Length; i++)
            {
                var currentString = strings[i];
                result += char.ToUpperInvariant(currentString[0]) +
                          currentString.Substring(1, currentString.Length - 1);
            }

            return result;
        }

        public static IEnumerable<string> GetAllArrayField(string className)
        {
            return GetType(className)
                ?.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(info => info.FieldType.IsArray).Select(info => info.Name);
        }
        
        public static IEnumerable<string> GetAllFields(string className)
        {
            return GetType(className)
                ?.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(info => info.Name);
        }

        public static IEnumerable<string> GetAllMethod(string className)
        {
            return GetType(className)
                ?.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                             BindingFlags.DeclaredOnly).Select(method => method.Name);
        }
        
        public static object GetDefaultValue(Type enumType)
        {
            var attribute = enumType.GetCustomAttribute<DefaultValueAttribute>(inherit: false);
            if (attribute != null)
                return attribute.Value;

            var innerType = enumType.GetEnumUnderlyingType();
            var zero = Activator.CreateInstance(innerType);
            if (enumType.IsEnumDefined(zero))
                return zero;

            var values = enumType.GetEnumValues();
            return values.GetValue(0);
        }

        public static ConverterInfo TryGetConverter(FieldInfo field)
        {
            var attrib = field.GetCustomAttribute<ConverterAttribute>(true);

            if (attrib == null)
            {
                return default;
            }

            var converterType = attrib.Type;
            var fromType = attrib.FromType;
            var toType = field.FieldType;
            var lineNumber = attrib.CallerLineNumber;
            var filePath = attrib.CallerFilePath;
            var iconverterType = typeof(IConvert<,>);

            if (converterType == null)
            {
                throw new ArgumentNullException($"Converter type cannot be null. File: {filePath}:{lineNumber}");
            }

            if (converterType.IsInterface || (converterType.IsAbstract && converterType.IsSealed == false))
            {
                throw new NotSupportedException(
                    $"{converterType.FullName} cannot be neither an interface nor an abstract class. " +
                    $"File: {filePath}:{lineNumber}"
                );
            }

            if (converterType.IsValueType == false && converterType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException(
                    $"{converterType.FullName} must have a parameterless constructor. " +
                    $"File: {filePath}:{lineNumber}"
                );
            }

            var interfaces = converterType.GetInterfaces()
                .Where(x => x.ImplementsOpenGenericInterface(iconverterType))
                .Where(x => {
                    var typeArgs = x.GetArgumentsOfInheritedOpenGenericInterface(iconverterType);
                    return typeArgs.Length == 2
                        && typeArgs[1] == toType
                        && (fromType == null || typeArgs[0] == fromType);
                })
                .ToArray();

            if (interfaces.Length < 1)
            {
                throw new NotImplementedException(
                    $"{converterType.FullName} must implement CsvReader.IConvert<{fromType?.ToString() ?? "TFrom"}, {toType}> interface. " +
                    $"File: {filePath}:{lineNumber}"
                );
            }

            if (fromType == null)
            {
                var typeArgs = interfaces[0].GetArgumentsOfInheritedOpenGenericInterface(iconverterType);
                fromType = typeArgs[0];
            }

            var isStatic = converterType.IsAbstract && converterType.IsSealed;
            var methods = converterType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            foreach (var method in methods)
            {
                if (method.ReturnType != toType)
                {
                    continue;
                }

                var parameters = method.GetParameters();

                if (parameters.Length != 1)
                {
                    continue;
                }

                if (parameters[0].ParameterType != typeof(object))
                {
                    continue;
                }

                return new ConverterInfo(converterType, fromType, method, isStatic);
            }

            throw new NotImplementedException(
                $"Cannot find applicable Convert method on type {converterType.FullName}." +
                $"File: {filePath}:{lineNumber}"
            );
        }
    }

    public readonly struct ConverterInfo
    {
        public readonly bool IsStatic;
        public readonly Type Type;
        public readonly Type FromType;
        public readonly MethodInfo Method;

        public bool IsValid
            => Type != null && FromType != null && Method != null;

        public ConverterInfo([NotNull] Type type, Type fromType, [NotNull] MethodInfo method, bool isStatic)
        {
            IsStatic = isStatic;
            Type = type;
            FromType = fromType;
            Method = method;
        }

        public Type GetFieldType(FieldInfo field)
            => IsValid ? FromType : field.FieldType;

        public object Convert(object value)
        {
            if (IsValid == false)
                return value;

            if (IsStatic)
                return Method.Invoke(null, new[] { value });

            var instance = Activator.CreateInstance(Type);
            return Method.Invoke(instance, new[] { value });
        }
    }
}
#endif
