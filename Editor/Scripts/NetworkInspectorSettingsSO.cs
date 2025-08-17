using UnityEngine;

namespace UnityFetch.Editor.Scripts
{
    [CreateAssetMenu(fileName = "NetworkInspectorSettingsSO", menuName = "Scriptable Objects/NetworkInspectorSettingsSO")]
    public class NetworkInspectorSettingsSO : ScriptableObject
    {
        public bool clearOnPlay = true;
        public bool clearOnBuild = true;
        public bool clearOnRecompile = true;
    }
}
