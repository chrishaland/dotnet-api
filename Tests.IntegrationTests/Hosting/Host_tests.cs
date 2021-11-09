using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests.IntegrationTests.Hosting
{
    [TestFixture]
    public class Host_tests
    {
        [Test]
        public async Task Host_is_running_and_accepts_http_calls()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            var (response, _) = await SUT.SendHttpRequest(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
