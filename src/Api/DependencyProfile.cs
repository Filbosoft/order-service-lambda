using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api
{
    public static class DependencyProfile
    {
        public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsSection = configuration.GetSection("CORS");
            var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>();

            services.AddCors(options => 
                options.AddDefaultPolicy(builder => 
                    builder.WithOrigins(allowedOrigins)));
            
            return services;
        }
    }
}