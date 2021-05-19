using System;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;
using static Acceptance.Seeds.CurrencySeeds;
using static Acceptance.Seeds.ExchangeSeeds;

namespace Acceptance.Seeds
{
    public static class AssetSeeds
    {
        public static readonly AssetEntity DKK_STOCK = new AssetEntity
        {
            Name = "DKK Stock",
            Symbol = "DKS",
            Type = AssetType.Stock,
            Exchange = DKK_EXCHANGE,
            Currency = DKK,
            LastUpdated = DateTime.UtcNow
        };

        public static readonly AssetEntity USD_STOCK = new AssetEntity
        {
            Name = "USD Stock",
            Symbol = "USDS",
            Exchange = USD_EXCHANGE,
            Currency = USD,
            LastUpdated = DateTime.UtcNow
        };
    }
}