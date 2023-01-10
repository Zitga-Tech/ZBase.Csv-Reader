using System;
using CsvReader;
using UnityEngine;

namespace Csv
{
    public class StageReward : ScriptableObject
    {
        public StageRewardItem[] rewards;

        [Serializable]
        public struct StageRewardItem
        {
            [CsvColumnIgnore]
            public int stId;
            
            [CsvColumnAttribute(ColumnName = "st_id")]
            public int stageId;

            public Resource[] rewards;
            
            [CsvColumnFormatAttribute(ColumnFormat = "fc_{0}")]
            public Resource[] firstClearRewards;
        }
    }
}