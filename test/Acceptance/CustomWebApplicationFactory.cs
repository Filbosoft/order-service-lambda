
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using API;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using System.Threading.Tasks;
using System.Net.Http;
using Amazon.Runtime;
using Acceptance.Utilities;

namespace Acceptance
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<Startup>, IDisposable
    {
        private IConfiguration Configuration;

        public CustomWebApplicationFactory()
        {
            /***
            * Gets the configuration from the appsettings.json placed in the Api/conf folder.
            ***/
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Testing";
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

        }

        public new void Dispose()
        {
            base.Dispose();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {

            });
        }

        public IAmazonDynamoDB GetDynamoDB()
        {
            var db = GetScopedService<IAmazonDynamoDB>();

            return db;
        }

        public IDynamoDBContext GetDynamoDBContext()
        {
            var dbContext = GetScopedService<IDynamoDBContext>();

            return dbContext;
        }

        public T GetScopedService<T>()
        {
            var scopeFactory = this.Services.GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();

            return scope.ServiceProvider.GetService<T>();
        }

        public HttpClient CreateAuthorizedClient()
        {
            var client = base.CreateClient();
            var accessToken = GetTestUserToken().Result;
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            return client;
        }

        public async Task<string> GetTestUserToken()
        {
            var cognitoTestConfig = Configuration.GetSection("Cognito")
                .Get<CognitoTestConfig>();

            var user = GetTestUser(cognitoTestConfig);
            var authRequest = new InitiateSrpAuthRequest{Password = cognitoTestConfig.TestUserPassword};

            var authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
            var token = authResponse.AuthenticationResult.IdToken;

            return token;
        }

        private CognitoUser GetTestUser(CognitoTestConfig config)
        {            
            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials());
            var userPool = new CognitoUserPool(
                config.UserPoolId,
                config.ClientId,
                provider);
            var user = new CognitoUser(
                config.TestUserId,
                config.ClientId,
                userPool,
                provider);

            return user;
        }
    }
}