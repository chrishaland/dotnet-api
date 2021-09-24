using Haland.DotNetTrace;

namespace Host
{
    public partial class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabase(Configuration);
            services.AddOpenApiDocumentation();

            services.AddAmqpService(Configuration)
                //.AddAmqpSubscriber<TestSubscriber, TestMessage>()
            ;

            services.AddAuthorizationAndPolicies(Configuration);
            services.AddOpenIdConnectAuthentication(Configuration);

            services.AddTracing();
            services.AddControllers().AddNewtonsoftJson();
        }
    }
}
