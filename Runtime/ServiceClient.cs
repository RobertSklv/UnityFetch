using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Collections;

namespace UnityFetch
{
    public abstract class ServiceClient
    {
        protected readonly UnityFetchClient client;

        public ServiceClient(UnityFetchClient client)
        {
            this.client = client;
        }

        protected Task RequestSimple<TRequestModel>(TRequestModel requestBody, params object[] parameters)
        {
            return AsyncRequestSimple<object>(new(), requestBody, exceptionBasedErrorHandling: true, parameters);
        }

        protected Task<TResponseModel> RequestSimple<TRequestModel, TResponseModel>(TRequestModel requestBody, params object[] parameters)
        {
            return AsyncRequestSimple<TResponseModel>(new(), requestBody, exceptionBasedErrorHandling: true, parameters);
        }

        protected Task<TResponseModel> RequestSimpleParamsOnly<TResponseModel>(params object[] parameters)
        {
            return AsyncRequestSimple<TResponseModel>(new(), null, exceptionBasedErrorHandling: true, parameters);
        }

        protected Task<TResponseModel> RequestSimple<TResponseModel>()
        {
            return AsyncRequestSimple<TResponseModel>(new(), null, exceptionBasedErrorHandling: true, new object[] { });
        }

        protected Task RequestSimpleParamsOnly(params object[] parameters)
        {
            return AsyncRequestSimple<object>(new(), null, exceptionBasedErrorHandling: true, parameters);
        }

        protected Task<UnityFetchResponse<object>> Request<TRequestModel>(TRequestModel requestBody, params object[] parameters)
        {
            return AsyncRequest<object>(new(), requestBody, exceptionBasedErrorHandling: false, parameters);
        }

        protected Task<UnityFetchResponse<TResponseModel>> Request<TRequestModel, TResponseModel>(TRequestModel requestBody, params object[] parameters)
        {
            return AsyncRequest<TResponseModel>(new(), requestBody, exceptionBasedErrorHandling: false, parameters);
        }

        protected Task<UnityFetchResponse<TResponseModel>> RequestParamsOnly<TResponseModel>(params object[] parameters)
        {
            return AsyncRequest<TResponseModel>(new(), null, exceptionBasedErrorHandling: false, parameters);
        }

        protected Task<UnityFetchResponse<TResponseModel>> Request<TResponseModel>()
        {
            return AsyncRequest<TResponseModel>(new(), null, exceptionBasedErrorHandling: false, new object[] { });
        }

        protected Task<UnityFetchResponse<object>> RequestParamsOnly(params object[] parameters)
        {
            return AsyncRequest<object>(new(), null, exceptionBasedErrorHandling: false, parameters);
        }

        protected UnityFetchCoroutineRequestWrapper<object> CoroutineRequest<TRequestModel>(
            TRequestModel requestBody,
            params object[] parameters)
        {
            return CoroutineRequest<object>(new(), requestBody, parameters);
        }

        protected UnityFetchCoroutineRequestWrapper<TResponseModel> CoroutineRequest<TRequestModel, TResponseModel>(TRequestModel requestBody, params object[] parameters)
        {
            return CoroutineRequest<TResponseModel>(new(), requestBody, parameters);
        }

        protected UnityFetchCoroutineRequestWrapper<TResponseModel> CoroutineRequest<TResponseModel>(params object[] parameters)
        {
            return CoroutineRequest<TResponseModel>(new(), null, parameters);
        }

        protected UnityFetchCoroutineRequestWrapper<TResponseModel> CoroutineRequest<TResponseModel>()
        {
            return CoroutineRequest<TResponseModel>(new(), null, new object[] { });
        }

        protected UnityFetchCoroutineRequestWrapper<object> CoroutineRequestParamsOnly(params object[] parameters)
        {
            return CoroutineRequest<object>(new(), null, parameters);
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

        private Task<UnityFetchResponse<T>> AsyncRequest<T>(
            StackTrace stackTrace,
            object body,
            bool exceptionBasedErrorHandling,
            params object[] parameters)
        {
            MethodBase actionMethod = stackTrace.GetFrame(1).GetMethod();

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

            if (IsDefinedInRoute(url, ":resource"))
            {
                url = AddToRoute(url, ":resource", controllerName);
            }
            else if (IsDefinedInRoute(url, ":resources"))
            {
                url = AddToRoute(url, ":resources", Util.CapitalizeFirstLetter(Util.PluralizeWord(controllerName)));
            }

            List<object> routeParams = new();
            Dictionary<string, object> queryParams = new();

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
                        routeParams.Add(parameters[p.Position]);
                    }
                    else
                    {
                        InQueryAttribute inQuery = p.GetCustomAttribute<InQueryAttribute>();
                        string paramName = inQuery == null || string.IsNullOrEmpty(inQuery.alias) ? p.Name : inQuery.alias;
                        queryParams.Add(paramName, parameters[p.Position]);
                    }
                }
            }

            return client.Request<T>(url, method, body, options =>
            {
                if (actionAttribute != null)
                {
                    if (actionAttribute.RetryCount != null) options.RetryCount = actionAttribute.RetryCount.Value;
                    if (actionAttribute.RetryDelay != null) options.RetryDelay = actionAttribute.RetryDelay.Value;
                }

                options.AddRouteParameters(routeParams);
                options.AddQueryParameters(queryParams);

                if (exceptionBasedErrorHandling)
                {
                    options.OnError((response) =>
                    {
                        throw new UnityFetchRequestException(response);
                    });
                }
            });
        }

        private async Task<T> AsyncRequestSimple<T>(
            StackTrace stackTrace,
            object body,
            bool exceptionBasedErrorHandling,
            params object[] parameters)
        {
            UnityFetchResponse<T> response = await AsyncRequest<T>(stackTrace, body, exceptionBasedErrorHandling, parameters);

            return response.content;
        }

        private UnityFetchCoroutineRequestWrapper<T> CoroutineRequest<T>(
            StackTrace stackTrace,
            object body,
            params object[] parameters)
        {
            Task<UnityFetchResponse<T>> requestTask = AsyncRequest<T>(stackTrace, body, exceptionBasedErrorHandling: false, parameters);
            UnityFetchCoroutineRequestWrapper<T> requestWrapper = new();

            void onSuccess(T obj)
            {
                requestWrapper?.onSuccess(obj);
                requestWrapper?.SetResponse(requestTask.Result);
            }

            void onError(UnityFetchResponse<T> res)
            {
                requestWrapper?.onError(res);
                requestWrapper?.SetResponse(requestTask.Result);
            }

            IEnumerator routine = client.CoroutineRequest(requestTask, onSuccess, onError);
            requestWrapper.routine = routine;

            return requestWrapper;
        }
    }
}