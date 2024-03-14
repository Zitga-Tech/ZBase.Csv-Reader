#if ANTI_CHEAT
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Csv
{
    public class ApiConfigAntiCheat : ScriptableObject
    {
        public Entry data;
        
        [Serializable]
        public struct Entry
        {
            public ObscuredString apiHost;
            public ObscuredInt apiPort;

            public ObscuredInt apiVersion;
            public ObscuredString apiSecretKey;
        }
    }
}
#endif

