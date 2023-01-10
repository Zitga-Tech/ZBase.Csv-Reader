using System;
using UnityEngine;

namespace Csv
{
    public class RatingDataList : ScriptableObject
    {
        public MapIdTrigger[] dataGroups;

        [Serializable]
        public class MapIdTrigger
        {
            public int mapId;
            public int difficulty;
        }
    }
}