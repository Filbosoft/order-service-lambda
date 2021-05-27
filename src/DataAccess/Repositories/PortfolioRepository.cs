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
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly RestClient client;

        public PortfolioRepository(IHttpContextAccessor httpContextAccessor, IOptions<PortfolioServiceOptions> serviceOptions)
        {
            var httpContext = httpContextAccessor?.HttpContext;
            var userToken = HttpContextHelper.GetTokenFromAuthorizationHeader(httpContext);
            var portfolioUrl = serviceOptions.Value.URL;

            client = new RestClient(portfolioUrl);
            client.Authenticator = new JwtAuthenticator(userToken);
            
        }
        public async Task<PortfolioDetail> GetPortfolioById(string portfolioId)
        {
            var request = new RestRequest(portfolioId, DataFormat.Json);
            var portfolio = await client.GetAsync<PortfolioDetail>(request);

            return portfolio;
        }
    }
}