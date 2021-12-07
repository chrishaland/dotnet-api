namespace Tests.IntegrationTests.Hosting;

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


    [Test]
    public async Task Host_is_ready()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/ready");
        var (response, _) = await SUT.SendHttpRequest(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }


    [Test]
    public async Task Host_is_live()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        var (response, _) = await SUT.SendHttpRequest(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
