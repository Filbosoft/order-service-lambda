using System.Collections.Generic;
using Conditus.Trader.Domain.Models;

using static Integration.Seeds.V1.AssetSeeds;

namespace Integration.Seeds.V1
{
    public static class PortfolioSeeds
    {
        public const int TESTUSERS_PORTFOLIO_STOCK_QUANTITY = 100;        
        public static readonly PortfolioDetail TESTUSER_PORTFOLIO = new PortfolioDetail
        {
            Id = "10b20561-de35-4bf4-84ee-b1452d431bcd",
            Name = "Testuser's portfolio",
            Capital = 10000M,
            Assets = new List<PortfolioAsset>
            {
                new PortfolioAsset{Symbol = DKK_STOCK.Symbol, Name = DKK_STOCK.Name, Quantity = TESTUSERS_PORTFOLIO_STOCK_QUANTITY }
            }
        };

        public static readonly PortfolioDetail NONTESTUSER_PORTFOLIO = new PortfolioDetail
        {
            Id = "83c61b7a-df2f-43de-9272-a9f307dc4768",
            Name = "Not Testuser's portfolio",
            Capital = 10000M
        };
    }
}