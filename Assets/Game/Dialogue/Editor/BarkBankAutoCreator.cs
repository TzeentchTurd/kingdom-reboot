using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Dialogue.Editor
{
    [InitializeOnLoad]
    public static class BarkBankAutoCreator
    {
        private const string AssetPath = "Assets/Game/Dialogue/BarkBank.asset";

        static BarkBankAutoCreator()
        {
            EnsureAsset();
        }

        private static void EnsureAsset()
        {
            var bank = AssetDatabase.LoadAssetAtPath<BarkBank>(AssetPath);
            if (bank != null) return;

            var dir = Path.GetDirectoryName(AssetPath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
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

            bank = ScriptableObject.CreateInstance<BarkBank>();
            AssetDatabase.CreateAsset(bank, AssetPath);
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();
        }
    }
}

