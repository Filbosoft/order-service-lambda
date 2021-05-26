using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business;
using Conditus.Trader.Domain.Models;

using static Integration.Seeds.V1.AssetSeeds;

namespace Integration.Fakes.V1
{
    public class AssetRepositoryFake : IAssetRepository
    {
        private static List<AssetDetail> Assets = new List<AssetDetail>
        {
            DKK_STOCK,
            USD_STOCK
        };

        public async Task<AssetDetail> GetAssetBySymbol(string symbol)
        {
            var asset = Assets.FirstOrDefault(a => a.Symbol.Equals(symbol));

            return await Task.FromResult(asset);
        }
    }
}