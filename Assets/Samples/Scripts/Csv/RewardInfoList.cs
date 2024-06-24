using System;
using CsvReader;
using UnityEngine;

namespace Csv
{
    public class RewardInfoList : ScriptableObject
    {
        public RewardStageItem[] dataGroups;
        
        [Serializable]
        public struct RewardStageItem
        {
            [Converter(typeof(CustomIntConverter))]
            public CustomInt stage;

            public RewardInfoItem[] rewardInfo;
        }
        
        [Serializable]
        public class RewardInfoItem
        {
            public bool isFirst;
            public Resource resource;
        }
    }

    [Serializable]
    public struct CustomInt
    {
        public int value;

        public static implicit operator CustomInt(int value)
        {
            return new CustomInt { value = value };
        }

        public static implicit operator int(CustomInt value)
        {
            return value.value;
        }
    }

    public readonly struct CustomIntConverter : IConvert<int, CustomInt>
    {
        public CustomInt Convert(object value)
        {
            return (int)value;
        }
    }
}
