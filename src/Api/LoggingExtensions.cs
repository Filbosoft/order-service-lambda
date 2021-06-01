using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Api
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// This extension method was implemented to configure logging in a single place.
        /// Because of the different web servers running the application hosted and locally,
        /// this implementation ensures that the logging works for both hosted and local execution
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>builder with logging configured</returns>
        public static IWebHostBuilder AddLogging(this IWebHostBuilder builder)
        {
            builder
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            return builder;
        }
    }
}