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
            string[] guids = AssetDatabase.FindAssets(
                "t:NetworkSO"
            );

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);

            NetworkSO networkSO = AssetDatabase.LoadAssetAtPath<NetworkSO>(path);
            networkSO.AddRequest(requestInfo);
            EditorUtility.SetDirty(networkSO);
        }

        private static void OnRequestFinish(UnityFetchRequestInfo requestInfo)
        {
            //TODO
        }
    }
}