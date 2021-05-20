using System.Collections.Generic;

namespace Conditus.Trader.Domain.Models
{
    public class PortfolioDetail
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Capital { get; set; }
        public List<PortfolioAsset> Assets { get; set; }
    }

    public class PortfolioOverview
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}