#if UNITY_EDITOR
using StillSky.Characters;
using UnityEditor;
using UnityEngine;

namespace StillSky.EditorTools
{
    /// <summary>
    /// Backup way to create CharacterStats assets from the top menu:
    /// Tools > Still Sky > Create Character Stats.
    /// </summary>
    public static class CharacterStatsCreator
    {
        private const string Folder = "Assets/_Project/Data/Characters";

        [MenuItem("Tools/Still Sky/Create Character Stats")]
        public static void Create()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                Debug.LogError($"Folder not found: {Folder}");
                return;
            }

            var stats = ScriptableObject.CreateInstance<CharacterStats>();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{Folder}/NewCharacterStats.asset");

            AssetDatabase.CreateAsset(stats, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = stats;          // select it so you can rename right away
            EditorGUIUtility.PingObject(stats);
            Debug.Log($"Created {path}");
        }
    }
}
#endif