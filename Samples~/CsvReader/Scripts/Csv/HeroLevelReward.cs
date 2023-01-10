using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Csv
{
    [Serializable]
    public class HeroLevelReward : SerializedScriptableObject
    {
        private HeroLevelRewardItem[] _dataGroups;

        public Dictionary<int, Resource[]> dataDict;

        [Serializable]
        public class HeroLevelRewardItem
        {
            public int level;
            public Resource[] resources;
        }

        public void Convert()
        {
            if (this._dataGroups == null)
            {
                return;
            }

            this.dataDict = new Dictionary<int, Resource[]>();

            foreach (HeroLevelRewardItem item in this._dataGroups)
            {
                this.dataDict.Add(item.level, item.resources);
            }
        }
    }
}