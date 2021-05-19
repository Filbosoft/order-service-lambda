using System.Threading.Tasks;
using Conditus.Trader.Domain.Models;

namespace Business
{
    public interface IPortfolioRepository
    {
        Task<PortfolioDetail> GetPortfolioById(string portfolioId);
    }
}