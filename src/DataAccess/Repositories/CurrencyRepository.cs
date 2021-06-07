using System.Threading.Tasks;
using Business.Repositories;
using DataAccess.Options;
using DateAccess.HelperMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace DataAccess.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly RestClient client;

        public CurrencyRepository(IHttpContextAccessor httpContextAccessor, IOptions<CurrencyServiceOptions> serviceOptions)
        {
            var httpContext = httpContextAccessor?.HttpContext;
            var userToken = HttpContextHelper.GetTokenFromAuthorizationHeader(httpContext);
            var portfolioUrl = serviceOptions.Value.URL;

            client = new RestClient(portfolioUrl);
            client.Authenticator = new JwtAuthenticator(userToken);
        }

        public async Task<decimal> ConvertCurrency(string fromCode, string toCode, decimal amount)
        {
            var request = new RestRequest();
            request.AddQueryParameter("fromSymbol", fromCode);
            request.AddQueryParameter("toSymbol", toCode);
            request.AddQueryParameter("amount", amount.ToString());

            var convertedCurrency = await client.GetAsync<decimal>(request);

            return convertedCurrency;
        }
    }
}