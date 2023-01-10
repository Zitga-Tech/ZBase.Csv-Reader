using System;
using System.ComponentModel;

namespace Csv
{
    [Serializable]
    public struct Resource
    {
        public int resourceType;
        public int resourceId;
        [DefaultValue(int.MaxValue)]
        public long resourceNumber;
    }
}