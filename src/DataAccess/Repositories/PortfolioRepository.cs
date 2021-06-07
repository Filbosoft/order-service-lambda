using System.Collections.Generic;
using System.Threading.Tasks;
using Business.Repositories;
using Conditus.Trader.Domain.Models;
using DataAccess.Options;
using DataAccess.Repositories.Responses;
using DateAccess.HelperMethods;
using Microsoft.AspNetCore.Http;
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

        private Dictionary<string, PortfolioDetail> PortfolioCache = new Dictionary<string, PortfolioDetail>();
        
        
        public async Task<PortfolioDetail> GetPortfolioById(string portfolioId)
        {
            var cachePortfolio = PortfolioCache.GetValueOrDefault(portfolioId);
            if (cachePortfolio != null)
                return cachePortfolio;
            
            var request = new RestRequest(portfolioId, DataFormat.Json);
            var apiResponse = await client.GetAsync<ApiResponse<PortfolioDetail>>(request);
            var portfolio = apiResponse.Data;

            PortfolioCache.Add(portfolioId, portfolio);

            return portfolio;
        }
    }
}