using System;
using Sirenix.OdinInspector;

namespace Csv
{
    [Serializable]
    public class HeroConfigItem
    {
        public int id;
        public float health;
        public float armor;
    }

    [Serializable]
    public class HeroConfig : SerializedScriptableObject
    {
        public HeroConfigItem[] items;
    }
}
