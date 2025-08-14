using System.Collections.Generic;
using UnityEngine;
using UnityFetch.Debugging;

namespace UnityFetch.Editor.Scripts
{
    public class NetworkSO : ScriptableObject
    {
        public List<UnityFetchRequestInfo> requests = new();

        public void AddRequest(UnityFetchRequestInfo request)
        {
            requests.Add(request);
        }

        public void ClearRequests()
        {
            requests.Clear();
        }
    }
}
