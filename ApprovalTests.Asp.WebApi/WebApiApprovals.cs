using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using ApprovalUtilities.Utilities;

namespace ApprovalTests.Asp.WebApi
{
    public class WebApiApprovals
    {
        /// <param name="registerWebApiConfigFn">Probably <c>WebApiConfig.Register</c></param>
        public static void VerifyRouting(Action<HttpConfiguration> registerWebApiConfigFn, HttpRequestMessageList httpRequestMessageList)
        {
            var config = new HttpConfiguration();

            registerWebApiConfigFn(config);
            config.EnsureInitialized();


            var stringBuilder = new StringBuilder();
            foreach (var request in httpRequestMessageList)
            {
                stringBuilder.Append(GetRoutingForRequest(request, config) + "\n");
            }
            Approvals.Verify(stringBuilder.ToString());
        }

        private static void RemoveOptionalRoutingParameters(IDictionary<string, object> routeValues)
        {
            var optionalParams = routeValues.Where(x => x.Value == RouteParameter.Optional).Select(x => x.Key).ToList();

            foreach (var key in optionalParams)
            {
                routeValues.Remove(key);
            }
        }


        private static string GetRoutingForRequest(HttpRequestMessage request, HttpConfiguration config)
        {
            var handling = FormatRequest(request);

            FindRouting(request, config, handling);
            return handling.ToString();
        }

        private static void FindRouting(HttpRequestMessage request, HttpConfiguration config, RequestHandling handling)
        {
            var routeData = config.Routes.GetRouteData(request);
            if (routeData == null)
            {
                handling.Handler = "No route found";
            }
            var httpControllerDescriptor = GetHttpControllerDescriptor(request, config, handling, routeData);

            if (handling.Handler == null)
            {
                handling.Handler = GetHandlerText(request, config, routeData, httpControllerDescriptor);
            }
        }

        private static string GetHandlerText(HttpRequestMessage request, HttpConfiguration config, IHttpRouteData routeData, HttpControllerDescriptor httpControllerDescriptor)
        {
            var controllerContext = new HttpControllerContext(config, routeData, request) {RouteData = routeData, ControllerDescriptor = httpControllerDescriptor};

            var actionMapping = new ApiControllerActionSelector().SelectAction(controllerContext);
            var text = string.Format("Handled By: {0}.{1}({2})", actionMapping.ControllerDescriptor.ControllerType.FullName, actionMapping.ActionName, GetParameterString(actionMapping.ActionBinding.ParameterBindings, controllerContext.RouteData));
            return text;
        }

        private static string GetParameterString(HttpParameterBinding[] parameterBindings, IHttpRouteData routeData)
        {

            return parameterBindings.JoinStringsWith(b => b.Descriptor.ParameterName + "=" + routeData.Values.GetValueOrDefault(b.Descriptor.ParameterName), ", ");
        }

        private static HttpControllerDescriptor GetHttpControllerDescriptor(HttpRequestMessage request, HttpConfiguration config, RequestHandling handling, IHttpRouteData routeData)
        {
            if (handling.Handler != null)
            {
                return null;
            }

            RemoveOptionalRoutingParameters(routeData.Values);
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
            try
            {
                return new DefaultHttpControllerSelector(config).SelectController(request);
            }
            catch (HttpResponseException ex)
            {
                handling.Handler = string.Format("{0}: {1}", (int) ex.Response.StatusCode, ex.Response.ReasonPhrase);
                return null;
            }
        }

        private static RequestHandling FormatRequest(HttpRequestMessage request)
        {
            RequestHandling handling = new RequestHandling();
            if (!request.RequestUri.IsAbsoluteUri)
            {
                request.RequestUri = new Uri("http://example.com/" + request.RequestUri.OriginalString);
            }
            var requestString = request.ToString();
            handling.Request = requestString.Replace("\r\n{\r\n}", "{}");
            return handling;
        }
    }

    public static class DictionaryUtils
    {
        public static V GetValueOrDefault<K,V>(this IDictionary<K, V> map, K key)
        {
            if (map.ContainsKey(key))
            {
                return map[key];
            }
            else
            {
                return default(V);
            }
        }
    }
    public class RequestHandling
    {
        public string Request;
        public string Handler;

        public override string ToString()
        {
            return "{0}\n{1}\n".FormatWith(Request, Handler);
        }
    }
}