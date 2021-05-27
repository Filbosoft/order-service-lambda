using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Business.Repositories;
using DataAccess.Options;
using DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    public static class DependencyProfile
    {
        public static IServiceCollection AddDataAccessDependencies(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddDynamoDB(config)
                .AddDataAccessOptions(config)
                .AddRepositories();

            return services;
        }

        private static IServiceCollection AddDynamoDB(this IServiceCollection services, IConfiguration config)
        {
            var dynamoConfig = config.GetSection("DynamoDB");
            var isLocalMode = dynamoConfig.GetValue<bool>("LocalMode");

            if (isLocalMode)
            {
                services.AddScoped<IAmazonDynamoDB>(sp =>
                {
                    var clientConfig = new AmazonDynamoDBConfig { ServiceURL = dynamoConfig.GetValue<string>("LocalServiceUrl") };
                    return new AmazonDynamoDBClient(clientConfig);
                });
            }
            else
                services.AddAWSService<IAmazonDynamoDB>();

            services.AddScoped<IDynamoDBContext, DynamoDBContext>();

            return services;
        }

        private static IServiceCollection AddDataAccessOptions(this IServiceCollection services, IConfiguration config)
        {
            var servicesSection = config.GetSection("Services");

            services
                .Configure<PortfolioServiceOptions>(servicesSection.GetSection("PortfolioService"))
                .Configure<AssetServiceOptions>(servicesSection.GetSection("AssetService"));

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services
                .AddScoped<IPortfolioRepository, PortfolioRepository>()
                .AddScoped<IAssetRepository, AssetRepository>();

            return services;
        }
    }
}
