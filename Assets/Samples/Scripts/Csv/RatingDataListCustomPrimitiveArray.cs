using System;
using CsvReader;
using UnityEngine;

namespace Csv
{
    public class RatingDataListCustomPrimitiveArray : ScriptableObject
    {
        public MapIdTrigger[] dataGroups;

        [CsvClassCustomPrimitiveArray('~')]
        [Serializable]
        public class MapIdTrigger
        {
            public int mapId;
            public int[] difficulty;
            public string[] description;
        }
    }
}