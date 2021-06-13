using System.Collections.Generic;
using Conditus.Trader.Domain.Models;

using static Integration.Seeds.V1.AssetSeeds;
using static Integration.Seeds.V1.CurrencySeeds;

namespace Integration.Seeds.V1
{
    public static class PortfolioSeeds
    {
        public const int TESTUSERS_PORTFOLIO_STOCK_QUANTITY = 100;        
        public static readonly PortfolioDetail USER_DKK_PORTFOLIO = new PortfolioDetail
        {
            Id = "10b20561-de35-4bf4-84ee-b1452d431bcd",
            Name = "Users portfolio",
            Capital = 10000M,
            Assets = new List<PortfolioAsset>
            {
                new PortfolioAsset{Symbol = DKK_STOCK.Symbol, Name = DKK_STOCK.Name, Quantity = TESTUSERS_PORTFOLIO_STOCK_QUANTITY }
            },
            CurrencyCode = DKK.Code
        };
        public static readonly PortfolioDetail PAGINATION_PORTFOLIO = new PortfolioDetail
        {
            Id = "ae54f603-6887-4fe7-8f37-ffad20ce8f17",
            Name = "User portfolio for pagination tests",
            Capital = 10000M,
            CurrencyCode = DKK.Code
        };

        public static readonly PortfolioDetail NONUSER_PORTFOLIO = new PortfolioDetail
        {
            Id = "83c61b7a-df2f-43de-9272-a9f307dc4768",
            Name = "Not users portfolio",
            Capital = 10000M
        };
    }
}