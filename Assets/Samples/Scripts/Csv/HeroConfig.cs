using System;
using Sirenix.OdinInspector;

namespace Csv
{
    public enum HeroType
    {
        None,
        Warrior,
        Ranger,
        Healer,
    }
    
    [Serializable]
    public class HeroConfigItem
    {
        public int id;
        public HeroType type;
        public float health;
        public float armor;
    }

    [Serializable]
    public class HeroConfig : SerializedScriptableObject
    {
        public HeroConfigItem[] items;
    }
}
