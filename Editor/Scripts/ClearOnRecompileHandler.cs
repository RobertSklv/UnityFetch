using UnityEditor;

namespace UnityFetch.Editor.Scripts
{
    [InitializeOnLoad]
    public static class ClearOnRecompileHandler
    {
        static ClearOnRecompileHandler()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeRecompile;
        }

        private static void OnBeforeRecompile()
        {
            NetworkInspectorSettingsSO settings = StaticScriptableObjectLoader.Load<NetworkInspectorSettingsSO>();

            if (settings.clearOnRecompile)
            {
                NetworkSO networkSO = StaticScriptableObjectLoader.Load<NetworkSO>();
                networkSO.ClearRequests();
                EditorUtility.SetDirty(networkSO);
            }
        }
    }
}