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

        public void UpdateRequest(UnityFetchRequestInfo request)
        {
            UnityFetchRequestInfo existing = requests.Find(r => r.guid == request.guid);

            if (existing == null) return;

            int index = requests.IndexOf(existing);
            requests.RemoveAt(index);
            requests.Insert(index, request);
        }

        public void ClearRequests()
        {
            requests.Clear();
        }
    }
}
