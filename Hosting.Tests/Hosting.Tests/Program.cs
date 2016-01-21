using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hosting.Tests
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                RunTestsAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Completed.");
        }

        public static async Task RunTestsAsync()
        {
            using (var client = new HttpClient(new ConsoleLoggingHandler(), true))
            {
                client.BaseAddress = new Uri("http://localhost:8000");

                // SMS
                await client.PostAsync("/sms/api/sms/unicorn/hello", new StringContent(string.Empty));

                await client.GetAsync("/sms/api/sms/unicorn");

                // Counter
                await client.PostAsync("/counter/api/counter", new StringContent(string.Empty));

                await client.GetAsync("/counter/api/counter");

                await client.GetAsync("/Hosting/CounterService/api/counter");

                var request = new HttpRequestMessage(HttpMethod.Get, "/api/counter");
                request.Headers.Add("SF-ServiceName", "fabric:/Hosting/CounterService");
                await client.SendAsync(request);
            }
        }

        private sealed class ConsoleLoggingHandler : DelegatingHandler
        {
            public ConsoleLoggingHandler()
            {
                InnerHandler = new HttpClientHandler();
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var stopWatch = Stopwatch.StartNew();

                var response = await base.SendAsync(request, cancellationToken);

                stopWatch.Stop();

                Console.WriteLine($"Status: {response.StatusCode} Method: {response.RequestMessage.Method} URL: {response.RequestMessage.RequestUri} Time elapsed: {stopWatch.Elapsed}");

                return response;
            }
        }
    }
}
