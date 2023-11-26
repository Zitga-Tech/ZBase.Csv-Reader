using System;
using System.Collections.Generic;
using System.Reflection;

namespace CsvDownloader
{
    public static class CsvDownloaderUtils
    {
        
        public const string Comment = "$";
        public const string IndexDelimiter = ":";
        public const string SheetNameDelimiter = ".";
        
        // TODO: in .net standard 2.1 this is not needed
        public static readonly string[] IndexDelimiterArray = { IndexDelimiter };

        /// <summary>
        /// Split SheetName.SubName format.
        /// </summary>
        public static (string name, string subName) ParseSheetName(string name)
        {
            int idx = name.IndexOf(SheetNameDelimiter, StringComparison.Ordinal);

            if (idx == -1)
                return (name, null);

            return (name.Substring(0, idx), name.Substring(idx + 1));
        }

        /// <summary>
        /// Iterate properties with both getter and setter.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetEligibleProperties(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance |
                                              BindingFlags.DeclaredOnly;

            while (type != null)
            {
                var properties = type.GetProperties(bindingFlags);

                foreach (var property in properties)
                {
                    if (property.IsDefined(typeof(NonSerializedAttribute)))
                        continue;

                    if (property.GetMethod != null && property.SetMethod != null)
                        yield return property;
                }

                type = type.BaseType;
            }
        }
    }

}

