using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Repositories;
using Conditus.Trader.Domain.Models;

using static Integration.Seeds.V1.PortfolioSeeds;

namespace Integration.Fakes.V1
{
    public class PortfolioRepositoryFake : IPortfolioRepository
    {
        private static List<PortfolioDetail> TestUserPortfolios = new List<PortfolioDetail>
        {
            TESTUSER_PORTFOLIO
        };

        public async Task<PortfolioDetail> GetPortfolioById(string portfolioId)
        {
            var foundPortfolio = TestUserPortfolios.FirstOrDefault(p => p.Id.Equals(portfolioId));

            return await Task.FromResult(foundPortfolio);
        }
    }
}