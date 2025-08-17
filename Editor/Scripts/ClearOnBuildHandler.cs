using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor;

namespace UnityFetch.Editor.Scripts
{
    public class ClearOnBuildHandler : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            NetworkInspectorSettingsSO settings = StaticScriptableObjectLoader.Load<NetworkInspectorSettingsSO>();

            if (settings.clearOnBuild)
            {
                NetworkSO networkSO = StaticScriptableObjectLoader.Load<NetworkSO>();
                networkSO.ClearRequests();
                EditorUtility.SetDirty(networkSO);
            }
        }
    }
}