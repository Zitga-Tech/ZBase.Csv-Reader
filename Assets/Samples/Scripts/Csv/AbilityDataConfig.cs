using System;
using System.ComponentModel;
using UnityEngine;

namespace Csv
{
    public class AbilityDataConfig : ScriptableObject
    {
        [Header("Dash")]
        public DashAbilityData[] dashAbilityData;
        [Header("Fireball")]
        public FireballAbilityData[] fireballAbilityData;
        [Header("Explode")]
        public ExplodeAbilityData[] explodeAbilityData;
        [Header("Revive")]
        public ReviveAbilityData[] reviveAbilityData;
    }
    
    [DefaultValue(Revive)]
    public enum AbilityType
    {
        Fireball = 0,
        Dash = 1,
        Explode = 2,
        Revive = 3
    }
    
    public abstract class AbilityData
    {
        public int dataId;
        public AbilityType id;
    }
    
    [Serializable]
    public class DashAbilityData : AbilityData
    {

    }

    [Serializable]
    public class ExplodeAbilityData : AbilityData
    {
        #region Members

        public float castRange;

        #endregion Members

    }

    [Serializable]
    public class ReviveAbilityData : AbilityData
    {

    }
    
    public enum ModifierType
    {
        None = 0,
        Stun = 1,
        Knockback = 2,
        Bleed = 3,
        Heal = 4,
    }
    
    [Serializable]
    public struct ModifierIdentify
    {
        public ModifierType modifierType;
        public int modifierDataId;
    }
    
    [Serializable]
    public class FireballAbilityData : AbilityData
    {
        #region Members

        public float damageFactor;
        public ModifierIdentify[] modifierIdentifies;

        #endregion Members
    }
}