using System.Threading.Tasks;
using Business.Repositories;
using Conditus.Trader.Domain.Models;
using DataAccess.Options;
using DateAccess.HelperMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace DataAccess.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly RestClient client;

        public AssetRepository(IHttpContextAccessor httpContextAccessor, IOptions<AssetServiceOptions> serviceOptions)
        {
            var httpContext = httpContextAccessor?.HttpContext;
            var userToken = HttpContextHelper.GetTokenFromAuthorizationHeader(httpContext);
            var portfolioUrl = serviceOptions.Value.URL;

            client = new RestClient(portfolioUrl);
            client.Authenticator = new JwtAuthenticator(userToken);
        }

        public async Task<AssetDetail> GetAssetBySymbol(string symbol)
        {
            var request = new RestRequest();
            request.AddQueryParameter("Symbol", symbol);

            var asset = await client.GetAsync<AssetDetail>(request);

            return asset;
        }
    }
}