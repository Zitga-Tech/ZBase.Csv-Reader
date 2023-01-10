using System;

namespace Csv
{
    [Serializable]
    public struct QuestData
    {
        public int questId;
        public int questType;
        public int level;
        public int numberRequire;
        public Resource[] rewards;
    }
}