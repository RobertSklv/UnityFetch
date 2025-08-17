using UnityEditor;
using UnityFetch.Debugging;

namespace UnityFetch.Editor.Scripts
{
    [InitializeOnLoad]
    public static class RequestSubscriber
    {
        static RequestSubscriber()
        {
            UF.OnRequestStart += OnRequestStart;
            UF.OnRequestFinish += OnRequestFinish;
        }

        private static void OnRequestStart(UnityFetchRequestInfo requestInfo)
        {
            NetworkSO networkSO = StaticScriptableObjectLoader.Load<NetworkSO>();
            networkSO.AddRequest(requestInfo);
            EditorUtility.SetDirty(networkSO);
        }

        private static void OnRequestFinish(UnityFetchRequestInfo requestInfo)
        {
            NetworkSO networkSO = StaticScriptableObjectLoader.Load<NetworkSO>();
            networkSO.UpdateRequest(requestInfo);
            EditorUtility.SetDirty(networkSO);
        }
    }
}