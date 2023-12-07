using System;
using UnityEngine;

namespace Csv
{
    public class ApiConfig : ScriptableObject
    {
        public Entry data;
        
        [Serializable]
        public struct Entry
        {
            public string apiHost;
            public int apiPort;

            public int apiVersion;
            public string apiSecretKey;
        }
    }
}

