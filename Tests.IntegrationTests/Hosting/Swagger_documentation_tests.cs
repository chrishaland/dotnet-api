namespace Tests.IntegrationTests.Hosting;

[TestFixture]
public class Swagger_documentation_tests
{
    [Test]
    public async Task Swagger_docs_availability()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/docs/index.html");
        var (response, contentString) = await SUT.SendHttpRequest(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            StringAssert.Contains("<title>Swagger UI</title>", contentString);
        });
    }

    [Test]
    public async Task Swagger_docs_definition_availability()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/swagger/v1/swagger.json");
        var (response, contentString) = await SUT.SendHttpRequest(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }
}
