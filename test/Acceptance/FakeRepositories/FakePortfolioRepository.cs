using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business;
using Conditus.Trader.Domain.Models;

using static Acceptance.Seeds.PortfolioSeeds;

namespace Acceptance.FakeRepositories
{
    public class FakePortfolioRepository : IPortfolioRepository
    {
        private static List<PortfolioDetail> TestUserPortfolios = new List<PortfolioDetail>
        {
            TESTUSERS_PORTFOLIO
        };

        public async Task<PortfolioDetail> GetPortfolioById(string portfolioId)
        {
            var foundPortfolio = TestUserPortfolios.FirstOrDefault(p => p.Id.Equals(portfolioId));

            return await Task.FromResult(foundPortfolio);
        }
    }
}