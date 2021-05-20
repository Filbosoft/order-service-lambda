using System;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;
using static Acceptance.Seeds.CurrencySeeds;
using static Acceptance.Seeds.ExchangeSeeds;

namespace Acceptance.Seeds
{
    public static class AssetSeeds
    {
        public static readonly AssetDetail DKK_STOCK = new AssetDetail
        {
            Name = "DKK Stock",
            Symbol = "DKS",
            Currency = DKK,
            Type = AssetType.Stock
        };

        public static readonly AssetDetail USD_STOCK = new AssetDetail
        {
            Name = "USD Stock",
            Symbol = "USDS",
            Currency = USD,
            Type = AssetType.Stock
        };
    }
}