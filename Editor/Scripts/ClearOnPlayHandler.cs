using UnityEditor;

namespace UnityFetch.Editor.Scripts
{
    [InitializeOnLoad]
    public static class ClearOnPlayHandler
    {
        static ClearOnPlayHandler()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                NetworkInspectorSettingsSO settings = StaticScriptableObjectLoader.Load<NetworkInspectorSettingsSO>();

                if (settings.clearOnPlay)
                {
                    NetworkSO networkSO = StaticScriptableObjectLoader.Load<NetworkSO>();
                    networkSO.ClearRequests();
                    EditorUtility.SetDirty(networkSO);
                }
            }
        }
    }
}