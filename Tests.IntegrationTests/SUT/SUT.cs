using Newtonsoft.Json;
using Repository;
using System.Text;

namespace Tests.IntegrationTests
{
    public static class SUT
    {
        internal static Database Database => OneTimeTestServerSetup.Database;

        internal static async Task<(HttpResponseMessage message, T? content)> SendHttpRequest<T>(HttpRequestMessage request, object? data = null) where T : class
        {
            var (response, contentString) = await SendHttpRequest(request, data);

            T? content = null;
            if (!string.IsNullOrEmpty(contentString))
            {
                content = JsonConvert.DeserializeObject<T>(contentString);
            }

            return (response, content);
        }

        internal static async Task<(HttpResponseMessage message, string contentString)> SendHttpRequest(HttpRequestMessage request, object? data = null)
        {
            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await OneTimeTestServerSetup.Client.SendAsync(request);
            var contentString = await response.Content.ReadAsStringAsync();

            return (response, contentString);
        }
    }
}
