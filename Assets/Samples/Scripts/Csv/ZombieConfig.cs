using System;
using Sirenix.OdinInspector;

namespace Csv
{
    [Serializable]
    public class ZombieConfigItem
    {
        public int id;
        public float health;
        public float armor;
    }

    [Serializable]
    public class ZombieConfig : SerializedScriptableObject
    {
        public ZombieConfigItem[] items;
    }
}
