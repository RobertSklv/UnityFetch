using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace UnityFetch
{
    public abstract class ServiceClient
    {
        protected readonly UnityFetchClient client;

        public ServiceClient(UnityFetchClient client)
        {
            this.client = client;
        }

        protected Task Request<TRequestModel>(TRequestModel requestBody, params object[] parameters)
        {
            return MakeRequest<object>(new(), requestBody, parameters);
        }

        protected Task<TResponseModel> Request<TRequestModel, TResponseModel>(TRequestModel requestBody, params object[] parameters)
        {
            return MakeRequest<TResponseModel>(new(), requestBody, parameters);
        }

        protected Task<TResponseModel> Request<TResponseModel>(params object[] parameters)
        {
            return MakeRequest<TResponseModel>(new(), null, parameters);
        }

        protected Task<TResponseModel> Request<TResponseModel>()
        {
            return MakeRequest<TResponseModel>(new(), null, new object[] { });
        }

        protected Task Request(object requestBody, params object[] parameters)
        {
            return MakeRequest<object>(new(), requestBody, parameters);
        }

        protected Task<TResponseModel> RequestParametersOnly<TResponseModel>(params object[] parameters)
        {
            return MakeRequest<TResponseModel>(new(), null, parameters);
        }

        protected Task RequestParametersOnly(params object[] parameters)
        {
            return MakeRequest<object>(new(), null, parameters);
        }

        protected virtual string GetControllerName()
        {
            string typeName = GetType().Name;

            if (!typeName.EndsWith("Client"))
            {
                throw new UnityFetchException($"Invalid class name '{typeName}'. Service client classes must be suffixed with 'Client'.");
            }

            return typeName.Remove(typeName.LastIndexOf("Client"));
        }

        private bool IsDefinedInRoute(string route, string param)
        {
            return route != null && route.Contains('{' + param + '}');
        }

        private string AddToRoute(string route, string param, string value)
        {
            return route.Replace('{' + param + '}', value);
        }

        private async Task<T> MakeRequest<T>(StackTrace stackTrace, object body, params object[] parameters)
        {
            MethodBase actionMethod = stackTrace.GetFrame(3).GetMethod();

            ParameterInfo[] paramInfos = actionMethod.GetParameters();
            List<ParameterInfo> paramInfoList = new(paramInfos);

            if (body != null)
            {
                paramInfoList.RemoveAll(p => p.GetCustomAttribute<ActionParameterAttribute>() == null);
            }

            if (parameters.Length != paramInfoList.Count)
            {
                throw new Exception($"Argument count does not equal parameter count. Param count: {paramInfoList.Count}, Args count: {parameters.Length}");
            }

            ActionAttribute actionAttribute = actionMethod.GetCustomAttribute<ActionAttribute>();
            RequestMethod method = RequestMethod.GET;

            string url = null;
            string customActionName = null;

            if (actionAttribute != null)
            {
                url = actionAttribute.route;
                method = actionAttribute.method;
                customActionName = actionAttribute.Name;
            }

            string controllerName = GetControllerName();
            url ??= Util.UriCombine(controllerName, customActionName ?? actionMethod.Name);

            if (IsDefinedInRoute(url, "resource"))
            {
                url = AddToRoute(url, "resource", controllerName);
            }
            else if (IsDefinedInRoute(url, "resources"))
            {
                url = AddToRoute(url, "resources", Util.PluralizeWord(controllerName));
            }

            url = url?.ToLower();

            UnityFetchResponse<T> response = await client.Request<T>(url, method, body, options =>
            {
                foreach (ParameterInfo p in paramInfoList)
                {
                    if (IsDefinedInRoute(url, p.Name))
                    {
                        url = AddToRoute(url, p.Name, parameters[p.Position].ToString());
                    }
                    else
                    {
                        InRouteAttribute inRoute = p.GetCustomAttribute<InRouteAttribute>();
                        if (inRoute != null)
                        {
                            options.AddRouteParameter(parameters[p.Position]);
                        }
                        else
                        {
                            InQueryAttribute inQuery = p.GetCustomAttribute<InQueryAttribute>();
                            string paramName = inQuery == null || string.IsNullOrEmpty(inQuery.alias) ? p.Name : inQuery.alias;
                            options.AddQueryParameter(paramName, parameters[p.Position].ToString());
                        }
                    }
                }
            });

            return response.content;
        }
    }
}