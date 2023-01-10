using System;

namespace CsvReader
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class CsvClassCustomSeparator : Attribute
    {
        public readonly char CustomSeparator;

        public CsvClassCustomSeparator(char customSeparator = ',')
        {
            this.CustomSeparator = customSeparator;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class CsvClassCustomPrimitiveArray : Attribute
    {
        public readonly char CustomSeparator;

        public CsvClassCustomPrimitiveArray(char customSeparator = '~')
        {
            this.CustomSeparator = customSeparator;
        }
    }

    public class CsvColumnAttribute : Attribute
    {
        #region Properties

        public string ColumnName { get; set; }

        #endregion Properties
    }

    public class CsvColumnIgnore : Attribute
    {
    }

    public class CsvColumnFormatAttribute : Attribute
    {
        #region Properties

        public string ColumnFormat { get; set; }

        #endregion Properties
    }
}