using System.Threading.Tasks;
using Conditus.Trader.Domain.Models;

namespace Business.Repositories
{
    public interface IAssetRepository
    {
        Task<AssetDetail> GetAssetBySymbol(string symbol);
    }
}