using UnityEngine;

namespace Game.Juice
{
    [CreateAssetMenu(menuName = "Game/Juice Config", fileName = "JuiceConfig")]
    public class JuiceConfig : ScriptableObject
    {
        public int hitStopMs = 80;
        public float shakeAmplitude = 0.2f;
        public float shakeDuration = 0.15f;
        public float dmgNumberLifetime = 1.0f;

        private static JuiceConfig _instance;
        public static JuiceConfig Instance
        {
            get
            {
                if (_instance != null) return _instance;

#if UNITY_EDITOR
                // Try to load from known path in Editor
                var path = "Assets/Game/Juice/JuiceConfig.asset";
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<JuiceConfig>(path);
                if (_instance != null) return _instance;
#endif
                // Try Resources
                _instance = Resources.Load<JuiceConfig>("JuiceConfig");

                // Fallback to transient instance with defaults
                if (_instance == null)
                {
                    _instance = CreateInstance<JuiceConfig>();
                }
                return _instance;
            }
        }
    }
}

