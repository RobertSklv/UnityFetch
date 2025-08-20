using System.Collections.Generic;

namespace UnityFetch
{
    public abstract class CoroutineRestClient<TResource> : ServiceClient
    {
        public CoroutineRestClient(UnityFetchClient client)
            : base(client)
        {
        }

        [Action(RequestMethod.GET, route: "/{:resource}/{id}")]
        public UnityFetchCoroutineRequestWrapper<TResource> Get([InRoute] int id)
        {
            return CoroutineRequest<TResource>(id);
        }

        [Action(RequestMethod.POST, route: "/{:resource}")]
        public UnityFetchCoroutineRequestWrapper<TResource> Create(TResource resource)
        {
            return CoroutineRequest<TResource, TResource>(resource);
        }

        [Action(RequestMethod.PUT, route: "/{:resource}/{id}")]
        public UnityFetchCoroutineRequestWrapper<TResource> Update([InRoute] int id, TResource resource)
        {
            return CoroutineRequest<TResource, TResource>(resource, id);
        }

        [Action(RequestMethod.DELETE, route: "/{:resource}/{id}")]
        public UnityFetchCoroutineRequestWrapper<object> Delete([InRoute] int id)
        {
            return CoroutineRequestParamsOnly(id);
        }

        [Action(RequestMethod.GET, route: "/{:resources}")]
        public UnityFetchCoroutineRequestWrapper<List<TResource>> GetAll()
        {
            return CoroutineRequest<List<TResource>>();
        }
    }
}