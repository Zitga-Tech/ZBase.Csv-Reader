using System;
using Sirenix.OdinInspector;

namespace Csv
{
    public class RewardInfoList : SerializedScriptableObject
    {
        public RewardStageItem[] dataGroups;
        
        [Serializable]
        public struct RewardStageItem
        {
            public int stage;
            public RewardInfoItem[] rewardInfo;
        }
        
        [Serializable]
        public class RewardInfoItem
        {
            public bool isFirst;
            public Resource resource;
        }
    }
}
