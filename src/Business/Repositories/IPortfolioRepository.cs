using System.Threading.Tasks;
using Conditus.Trader.Domain.Models;

namespace Business.Repositories
{
    public interface IPortfolioRepository
    {
        Task<PortfolioDetail> GetPortfolioById(string portfolioId);
    }
}