using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API
{
    public static class DependencyProfile
    {
        public static IServiceCollection AddDynamoDB(this IServiceCollection services, IConfiguration config)
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
    }
}