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
            
            [CsvColumn(ColumnName = "st_id")]
            public int stageId;

            public Resource[] rewards;
            
            [CsvColumnFormat(ColumnFormat = "fc_{0}")]
            public Resource[] firstClearRewards;
        }
    }
}