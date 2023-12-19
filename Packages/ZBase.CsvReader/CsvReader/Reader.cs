#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace CsvReader
{
    public static class Reader
    {
        private static char s_csvSeparator;
        private static char s_primitiveArraySeparator;
        private static bool s_isCustomPrimitiveArray;
        public static object Deserialize(Type type, string text, string fieldSetValue, bool isArray = true)
        {
            s_csvSeparator = GetCsvSeparator(type);
            (s_isCustomPrimitiveArray, s_primitiveArraySeparator) = GetCsvPrimitiveArraySeparator(type);

            if (isArray)
            {
                return CreateArray(type, ParseCsv(text, s_csvSeparator), fieldSetValue);
            }
            else
            {
                return CreateSingle(type, ParseCsv(text, s_csvSeparator), fieldSetValue);
            }
        }

        private static Dictionary<string, int> CreateTable(List<string[]> rows)
        {
            Dictionary<string, int> table = new Dictionary<string, int>();

            for (int i = 0; i < rows[0].Length; i++)
            {
                string id = rows[0][i];
                if (CsvReaderUtils.IsValidKeyFormat(id))
                {
                    var camelId = CsvReaderUtils.ConvertSnakeCaseToCamelCase(id);

                    if (!table.ContainsKey(camelId))
                    {
                        table.Add(camelId, i);
                    }
                    else
                    {
                        throw new Exception("Key is duplicate: " + id);
                    }
                }
                else
                {
                    throw new Exception("Key is not valid: " + id);
                }
            }

            return table;
        }

        private static object CreateSingle(Type type, List<string[]> rows, string fieldSetValue)
        {
            var table = CreateTable(rows);
            
            return Create(1, 0, rows, table, type);
        }

        private static object CreateArray(Type type, List<string[]> rows, string fieldSetValue)
        {
            var (countElement, startRows) = CountNumberElement(1, 0, 0, rows);
            Array arrayValue = Array.CreateInstance(type, countElement);

            var table = CreateTable(rows);

            var isPrimitive = CsvReaderUtils.IsPrimitive(type);
            if (isPrimitive)
            {
                if(table.TryGetValue(fieldSetValue, out var idx))
                {
                    for (int i = 0; i < arrayValue.Length; i++)
                    {
                        object rowData = GetPrimitiveValue(type, rows[startRows[i]][idx]);
                        arrayValue.SetValue(rowData, i);
                    }
                }else
                {
                    throw new Exception($"Not found field to set value: {fieldSetValue}");
                }
            }
            else
            {
                for (int i = 0; i < arrayValue.Length; i++)
                {
                    object rowData = isPrimitive ? GetPrimitiveValue(type, rows[startRows[i]][0]) : Create(startRows[i], 0, rows, table, type);
                    arrayValue.SetValue(rowData, i);
                }
            }
            
            return arrayValue;
        }

        static object Create(int index, int parentIndex, List<string[]> rows, Dictionary<string, int> table, Type type, string format = "{0}")
        {
            object variable = Activator.CreateInstance(type);

            FieldInfo[] fieldInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var cols = rows[index];
            foreach (FieldInfo tmp in fieldInfo)
            {
                var ignoredAttributes = tmp.GetCustomAttributes(typeof(CsvColumnIgnore), true);
                if (ignoredAttributes.Length > 0) continue;

                bool isPrimitive = CsvReaderUtils.IsPrimitive(tmp, s_isCustomPrimitiveArray);
                if (isPrimitive)
                {
                    string csvColumnName = GetFieldCsvColumnName(tmp, format);
                    if (table.TryGetValue(csvColumnName, out var idx))
                    {
                        SetValuePrimitive(variable, tmp, idx < cols.Length ? cols[idx] : string.Empty);
                    }
                    else
                    {
                        Debug.LogError("Key is not exist: " + csvColumnName);
                    }
                }
                else
                {
                    var fieldFormat = GetCsvColumnFormat(tmp);

                    if (tmp.FieldType.IsArray)
                    {
                        var elementType = CsvReaderUtils.GetElementTypeFromFieldInfo(tmp);

                        string csvColumnName = GetFieldCsvColumnName(tmp, format);

                        int objectIndex;

                        var isElementPrimitive = CsvReaderUtils.IsPrimitive(elementType);

                        if (isElementPrimitive)
                        {
                            if (table.TryGetValue(csvColumnName, out var value))
                            {
                                objectIndex = value;
                                Assert.IsTrue(objectIndex < cols.Length);
                            }
                            else
                            {
                                throw new Exception($"Key is not exist: {csvColumnName}");
                            }
                        }
                        else
                        {
                            objectIndex = GetObjectIndex(elementType, table, fieldFormat);
                        }

                        var (countElement, startRows) = CountNumberElement(index, objectIndex, parentIndex, rows);

                        Array arrayValue = Array.CreateInstance(elementType, countElement);

                        for (int i = 0; i < arrayValue.Length; i++)
                        {
                            if (isElementPrimitive)
                            {
                                var value = rows[startRows[i]][objectIndex];
                                var primitiveValue = GetPrimitiveValue(elementType, value);
                                arrayValue.SetValue(primitiveValue, i);
                            }
                            else
                            {
                                var value = Create(startRows[i], objectIndex, rows, table, elementType, fieldFormat);
                                arrayValue.SetValue(value, i);
                            }
                        }

                        tmp.SetValue(variable, arrayValue);
                    }
                    else
                    {
                        var typeName = tmp.FieldType.FullName;
                        if (typeName == null)
                        {
                            throw new Exception("Full name is nil");
                        }

                        Type elementType = CsvReaderUtils.GetType(typeName);

                        var objectIndex = GetObjectIndex(elementType, table);

                        var value = Create(index, objectIndex, rows, table, elementType, fieldFormat);

                        tmp.SetValue(variable, value);
                    }
                }
            }

            return variable;
        }

        static void SetValuePrimitive(object variable, FieldInfo fieldInfo, string value)
        {
            var type = fieldInfo.FieldType;

            if (string.IsNullOrEmpty(value))
            {
                value = GetDefaultValue(fieldInfo);
            }

            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                string[] element = value.Split(s_primitiveArraySeparator);
                Array arrayValue = Array.CreateInstance(elementType ?? throw new InvalidOperationException(), element.Length);
                for (int i = 0; i < element.Length; i++)
                {
                    if (elementType == typeof(string))
                        arrayValue.SetValue(element[i], i);
                    else if (elementType.IsEnum)
                    {
                        arrayValue.SetValue(Enum.Parse(elementType, element[i]), i);
                    }
                    else
                    {
                        arrayValue.SetValue(Convert.ChangeType(element[i], elementType), i);
                    }
                }

                fieldInfo.SetValue(variable, arrayValue);
            }
            else
            {
                var primitiveValue = GetPrimitiveValue(type, value);
                fieldInfo.SetValue(variable, primitiveValue);
            }
        }

        private static object GetPrimitiveValue(Type type, string value)
        {
            if (type.IsEnum)
                return Enum.Parse(type, value);
            
            if (value.IndexOf('.') != -1 &&
                     (type == typeof(int) || type == typeof(long) ||
                      type == typeof(short)))
            {
                float f = (float) Convert.ChangeType(value, typeof(float));
                return Convert.ChangeType(f, type);
            }
            
            if (type == typeof(string))
                return value;
            
            return Convert.ChangeType(value, type);
        }

        private static List<string[]> ParseCsv(string text, char separator = ',')
        {
            List<string[]> lines = new List<string[]>();
            List<string> line = new List<string>();
            StringBuilder token = new StringBuilder();
            bool quotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                if (quotes)
                {
                    if ((text[i] == '\\' && i + 1 < text.Length && text[i + 1] == '\"') ||
                        (text[i] == '\"' && i + 1 < text.Length && text[i + 1] == '\"'))
                    {
                        token.Append('\"');
                        i++;
                    }
                    else switch (text[i])
                    {
                        case '\\' when i + 1 < text.Length && text[i + 1] == 'n':
                            token.Append('\n');
                            i++;
                            break;
                        case '\"':
                        {
                            line.Add(token.ToString());
                            token = new StringBuilder();
                            quotes = false;
                            if (i + 1 < text.Length && text[i + 1] == separator)
                                i++;
                            break;
                        }
                        default:
                            token.Append(text[i]);
                            break;
                    }
                }
                else if (text[i] == '\r' || text[i] == '\n')
                {
                    if (token.Length > 0)
                    {
                        line.Add(token.ToString());
                        token = new StringBuilder();
                    }

                    if (line.Count > 0)
                    {
                        lines.Add(line.ToArray());
                        line.Clear();
                    }
                }
                else if (text[i] == separator)
                {
                    line.Add(token.ToString());
                    token = new StringBuilder();
                }
                else if (text[i] == '\"')
                {
                    quotes = true;
                }
                else
                {
                    token.Append(text[i]);
                }
            }

            if (token.Length > 0)
            {
                line.Add(token.ToString());
            }

            if (line.Count > 0)
            {
                lines.Add(line.ToArray());
            }

            return lines;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private static int GetObjectIndex(Type type, Dictionary<string, int> table, string format = "{0}")
        {
            int minIndex = int.MaxValue;
            FieldInfo[] fieldInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo tmp in fieldInfo)
            {
                var fieldName = GetFieldCsvColumnName(tmp, format);
                if (table.TryGetValue(fieldName, out var idx))
                {
                    if (idx < minIndex)
                        minIndex = idx;
                }
                else
                {
                    //Debug.Log("Miss " + tmp.Name);
                }
            }

            return minIndex;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="objectIndex"></param>
        /// <param name="parentIndex"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        private static (int, List<int>) CountNumberElement(int rowIndex, int objectIndex, int parentIndex,
            List<string[]> rows)
        {
            int count = 0;
            var startRows = new List<int>();

            for (int i = rowIndex; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.Length > objectIndex && row[objectIndex].Equals(string.Empty) == false)
                {
                    if (objectIndex == parentIndex)
                    {
                        count++;
                        startRows.Add(i);
                    }
                    else if ( row.Length > parentIndex && row[parentIndex].Equals(string.Empty) || i == rowIndex)
                    {
                        count++;
                        startRows.Add(i);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return (count, startRows);
        }

        private static string GetFieldCsvColumnName(FieldInfo fieldInfo, string format)
        {
            object[] attributes = fieldInfo.GetCustomAttributes(typeof(CsvColumnAttribute), true);

            if (attributes.Length > 0)
            {
                CsvColumnAttribute csvColumnAttribute = (CsvColumnAttribute)attributes[0];
                return CsvReaderUtils.ConvertSnakeCaseToCamelCase(string.Format(format, csvColumnAttribute.ColumnName));
            }

            return CsvReaderUtils.ConvertSnakeCaseToCamelCase(string.Format(format, fieldInfo.Name));
        }

        private static string GetCsvColumnFormat(FieldInfo fieldInfo)
        {
            object[] attributes = fieldInfo.GetCustomAttributes(typeof(CsvColumnFormatAttribute), true);
            if (attributes.Length > 0)
            {
                CsvColumnFormatAttribute csvColumnFormatAttribute = (CsvColumnFormatAttribute)attributes[0];
                return csvColumnFormatAttribute.ColumnFormat;
            }

            return "{0}";
        }

        private static char GetCsvSeparator(Type type)
        {
            char separator = ',';
            object[] attributes = type.GetCustomAttributes(typeof(CsvClassCustomSeparator), true);
            if (attributes.Length > 0)
            {
                CsvClassCustomSeparator csvColumnFormatAttribute = (CsvClassCustomSeparator)attributes[0];
                separator = csvColumnFormatAttribute.CustomSeparator;
            }

            return separator;
        }

        private static (bool ,char) GetCsvPrimitiveArraySeparator(Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(CsvClassCustomPrimitiveArray), true);
            if (attributes.Length > 0)
            {
                CsvClassCustomPrimitiveArray csvColumnFormatAttribute = (CsvClassCustomPrimitiveArray)attributes[0];

                Assert.AreNotEqual(s_csvSeparator, csvColumnFormatAttribute.CustomSeparator);
                return (true, csvColumnFormatAttribute.CustomSeparator);
            }

            char defaultSeparator = '~';
            Assert.AreNotEqual(s_csvSeparator, defaultSeparator);

            return (false, defaultSeparator);
        }

        private static string GetDefaultValue(FieldInfo fieldInfo)
        {
            object[] attributes = fieldInfo.GetCustomAttributes(typeof(DefaultValueAttribute), true);
            if (attributes.Length > 0)
            {
                DefaultValueAttribute csvDefaultAttribute = (DefaultValueAttribute)attributes[0];

                return csvDefaultAttribute.Value.ToString();
            }

            var type = fieldInfo.FieldType;
            
            if (type == typeof(string))
            {
                return string.Empty;
            }
            if (type.IsNumeric())
            {
                return "0";
            }
            if (type == typeof(bool))
            {
                return "FALSE";
            }
            if (type.IsEnum)
            {
                return CsvReaderUtils.GetDefaultValue(type).ToString();
            }
                
            throw new Exception($"{typeof(Type).FullName} is not support default value. Current support (string, numeric, enum, true/false");
        }
    }
}
#endif
