using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityFetch
{
    public abstract class RestClient<TResource> : ServiceClient
    {
        public RestClient(UnityFetchClient client)
            : base(client)
        {
        }

        [Action(RequestMethod.GET, route: "/{:resource}/{id}")]
        public Task<TResource> Get([InRoute] int id)
        {
            return Request<TResource>(id);
        }

        [Action(RequestMethod.POST, route: "/{:resource}")]
        public Task<TResource> Create(TResource resource)
        {
            return Request<TResource, TResource>(resource);
        }

        [Action(RequestMethod.PUT, route: "/{:resource}/{id}")]
        public Task<TResource> Update([InRoute] int id, TResource resource)
        {
            return Request<TResource, TResource>(resource, id);
        }

        [Action(RequestMethod.DELETE, route: "/{:resource}/{id}")]
        public Task Delete([InRoute] int id)
        {
            return Request((object)id);
        }

        [Action(RequestMethod.GET, route: "/{:resources}")]
        public Task<List<TResource>> GetAll()
        {
            return Request<List<TResource>>();
        }
    }
}