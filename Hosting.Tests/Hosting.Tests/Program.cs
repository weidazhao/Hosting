using System;
using System.Net.Http;
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8000");

                // SMS
                LogResult(await client.PostAsync("/sms/api/sms/unicone/hello", new StringContent(string.Empty)));

                LogResult(await client.GetAsync("/sms/api/sms/unicone"));

                // Counter
                LogResult(await client.PostAsync("/counter/api/counter", new StringContent(string.Empty)));

                LogResult(await client.GetAsync("/counter/api/counter"));

                LogResult(await client.GetAsync("/Hosting/CounterService/api/counter"));

                var request = new HttpRequestMessage(HttpMethod.Get, "/api/counter");
                request.Headers.Add("SF-ServiceName", "fabric:/Hosting/CounterService");
                LogResult(await client.SendAsync(request));
            }
        }

        private static void LogResult(HttpResponseMessage response)
        {
            Console.WriteLine($"Status: {response.StatusCode} Method: {response.RequestMessage.Method} URL: {response.RequestMessage.RequestUri}");
        }
    }
}
