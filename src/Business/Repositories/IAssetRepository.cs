using System.Threading.Tasks;
using Conditus.Trader.Domain.Models;

namespace Business
{
    public interface IAssetRepository
    {
        Task<AssetDetail> GetAssetBySymbol(string assetId);
    }
}