using Repository;
using System.Text;
using System.Text.Json;

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
                var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(contentString));
                content = await JsonSerializer.DeserializeAsync<T>(contentStream, SerializerOptions);
            }

            return (response, content);
        }

        internal static async Task<(HttpResponseMessage message, string contentString)> SendHttpRequest(HttpRequestMessage request, object? data = null)
        {
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, SerializerOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await OneTimeTestServerSetup.Client.SendAsync(request);
            var contentString = await response.Content.ReadAsStringAsync();

            return (response, contentString);
        }

        private static JsonSerializerOptions SerializerOptions => new(JsonSerializerDefaults.Web);
    }
}
