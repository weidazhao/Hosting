using Microsoft.ServiceFabric.Services.Communication.Client;
using System.Fabric;
using System.Net.Http;

namespace Microsoft.ServiceFabric.AspNetCore.Gateway
{
    public class HttpRequestDispatcher : HttpClient, ICommunicationClient
    {
        public HttpRequestDispatcher()
            : base(new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false })
        {
        }

        public HttpRequestDispatcher(HttpMessageHandler handler)
            : base(handler)
        {
        }

        public HttpRequestDispatcher(HttpMessageHandler handler, bool disposeHandler)
            : base(handler, disposeHandler)
        {
        }

        #region ICommunicationClient

        string ICommunicationClient.ListenerName { get; set; }

        ResolvedServiceEndpoint ICommunicationClient.Endpoint { get; set; }

        ResolvedServicePartition ICommunicationClient.ResolvedServicePartition { get; set; }

        #endregion ICommunicationClient
    }
}
