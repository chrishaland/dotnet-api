using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;

namespace Host
{
    public static class DatabaseExtentions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Database");
            return services.AddDbContext<Database>(options =>
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    options.UseSqlServer(connectionString);
                }
                else
                {
                    options.UseInMemoryDatabase("Database");
                }
            });
        }
    }
}
