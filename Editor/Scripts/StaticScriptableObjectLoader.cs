using UnityEditor;
using UnityEngine;

namespace UnityFetch.Editor.Scripts
{
    public static class StaticScriptableObjectLoader
    {
        public static T Load<T>(string scriptableObjectAssetName = null)
            where T : ScriptableObject
        {
            scriptableObjectAssetName ??= typeof(T).Name;

            string[] guids = AssetDatabase.FindAssets(
                $"t:{scriptableObjectAssetName}"
            );

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            T so = AssetDatabase.LoadAssetAtPath<T>(path);

            return so;
        }
    }
}