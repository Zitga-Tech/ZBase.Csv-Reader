using System;
using System.Collections.Generic;
using System.Linq;
using CsvReader;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Csv
{
    [Serializable]
    public class StageInfoConfig : SerializedScriptableObject
    {
        private StageCostItem[] _costItems;

        private StageRewardItem[] _rewardConfigItems;

        private ReviveCostItem[] _reviveCostItems;

        private StageDroppableRewardItem[] _droppableRewardItems;

        private StageQuestConfigItem[] _questConfigItems;

        public Dictionary<int, StageInfoConfigItem> dataDict;

        [Serializable]
        public class StageInfoConfigItem
        {
            public Resource[] cost;

            public Resource[] rewards;

            public Resource[] firstClearRewards;

            public Resource[] reviveCost;

            public Resource[] droppableRewards;

            public Dictionary<int, QuestData[]> starQuests;
        }

        [Serializable]
        public struct StageCostItem
        {
            public int stageId;
            public Resource[] resources;
        }

        [Serializable]
        public struct ReviveCostItem
        {
            public int stageId;
            public Resource[] resources;
        }

        [Serializable]
        public struct StageRewardItem
        {
            public int stageId;

            public Resource[] rewards;

            [CsvColumnFormat(ColumnFormat = "fc_{0}")]
            public Resource[] firstClearRewards;
        }

        [Serializable]
        public struct DroppableReward
        {
            public int resourceType;
            public int resourceId;
            [CsvColumn(ColumnName = "max_amount")] public int resourceNumber;
        }

        [Serializable]
        public struct StageDroppableRewardItem
        {
            public int stageId;
            public DroppableReward[] resources;
        }

        [Serializable]
        public struct StageStarQuestConfigItem
        {
            public int star;
            public QuestData[] data;
        }

        [Serializable]
        public struct StageQuestConfigItem
        {
            public int stageId;
            public StageStarQuestConfigItem[] starQuests;
        }

        private StageInfoConfigItem GetOrAdd(int stageId)
        {
            this.dataDict ??= new Dictionary<int, StageInfoConfigItem>();

            if (this.dataDict.ContainsKey(stageId))
            {
                return this.dataDict[stageId];
            }

            var data = new StageInfoConfigItem();
            this.dataDict.Add(stageId, data);

            return data;
        }

        public void ConvertCost()
        {
            if (this._costItems == null)
            {
                return;
            }

            foreach (var item in this._costItems)
            {
                var stageInfo = GetOrAdd(item.stageId);
                stageInfo.cost = item.resources;
            }
        }

        public void ConvertRewards()
        {
            if (this._rewardConfigItems == null)
            {
                return;
            }

            foreach (var item in this._rewardConfigItems)
            {
                try
                {
                    var stageInfo = GetOrAdd(item.stageId);
                    stageInfo.rewards = item.rewards;
                    stageInfo.firstClearRewards = item.firstClearRewards;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        public void ConvertReviveCost()
        {
            if (this._reviveCostItems == null)
            {
                return;
            }

            foreach (var item in this._reviveCostItems)
            {
                try
                {
                    var stageInfo = GetOrAdd(item.stageId);
                    stageInfo.reviveCost = item.resources;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        public void ConvertDroppableRewards()
        {
            if (this._droppableRewardItems == null)
            {
                return;
            }

            foreach (var item in this._droppableRewardItems)
            {
                try
                {
                    var stageInfo = GetOrAdd(item.stageId);
                    stageInfo.droppableRewards = item.resources.Select(x => new Resource() {
                        resourceId = x.resourceId, resourceNumber = x.resourceNumber, resourceType = x.resourceType
                    }).ToArray();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        public void ConvertQuests()
        {
            if (this._questConfigItems == null)
            {
                return;
            }

            foreach (var item in this._questConfigItems)
            {
                try
                {
                    var stageInfo = GetOrAdd(item.stageId);
                    stageInfo.starQuests = new Dictionary<int, QuestData[]>();
                    foreach (StageStarQuestConfigItem stageStarQuestConfigItem in item.starQuests)
                    {
                        stageInfo.starQuests.Add(stageStarQuestConfigItem.star, stageStarQuestConfigItem.data);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }
    }
}