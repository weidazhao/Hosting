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

                Console.WriteLine("All passed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.Read();
        }

        public static async Task RunTestsAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8000");

                // SMS
                {
                    var response = await client.PostAsync("/sms/api/sms/userA/hello", new StringContent(string.Empty));
                    response.EnsureSuccessStatusCode();
                }
                {
                    var response = await client.GetAsync("/sms/api/sms/userA");
                    response.EnsureSuccessStatusCode();
                }

                // Counter       
                {
                    var response = await client.PostAsync("/counter/api/counter", new StringContent(string.Empty));
                    response.EnsureSuccessStatusCode();
                }
                {
                    var response = await client.GetAsync("/counter/api/counter");
                    response.EnsureSuccessStatusCode();
                }
                {
                    var response = await client.GetAsync("/Hosting/CounterService/api/counter");
                    response.EnsureSuccessStatusCode();
                }
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "/api/counter");
                    request.Headers.Add("SF-ServiceName", "fabric:/Hosting/CounterService");

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
