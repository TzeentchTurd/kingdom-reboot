using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Juice.Editor
{
    [InitializeOnLoad]
    public static class JuiceConfigAutoCreator
    {
        static JuiceConfigAutoCreator()
        {
            EnsureConfigAsset();
        }

        private const string AssetPath = "Assets/Game/Juice/JuiceConfig.asset";

        private static void EnsureConfigAsset()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<JuiceConfig>(AssetPath);
            if (cfg != null) return;

            // Ensure folder exists
            var dir = Path.GetDirectoryName(AssetPath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                // Create nested folders under Assets
                var parts = dir.Replace("\\", "/").Split('/');
                string path = "Assets";
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = parts[i];
                    if (!AssetDatabase.IsValidFolder(Path.Combine(path, next)))
                    {
                        AssetDatabase.CreateFolder(path, next);
                    }
                    path = Path.Combine(path, next);
                }
            }

            cfg = ScriptableObject.CreateInstance<JuiceConfig>();
            cfg.hitStopMs = 80;
            cfg.shakeAmplitude = 0.2f;
            cfg.shakeDuration = 0.15f;
            cfg.dmgNumberLifetime = 1.0f;

            AssetDatabase.CreateAsset(cfg, AssetPath);
            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
        }
    }
}

