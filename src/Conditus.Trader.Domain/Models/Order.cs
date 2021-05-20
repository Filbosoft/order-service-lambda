using System;
using Conditus.Trader.Domain.Enums;

namespace Conditus.Trader.Domain.Models
{
    public class OrderDetail
    {
        public string Id { get; set; }
        public string PortfolioId { get; set; }
        public OrderType Type { get; set; }
        public string AssetSymbol { get; set; }
        public AssetType AssetType { get; set; }
        public string AssetName { get; set; }
        public int Quantity { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class OrderOverview
    {
        public string Id { get; set; }
        public OrderType Type { get; set; }
        public string AssetSymbol { get; set; }
        public AssetType AssetType { get; set; }
        public string AssetName { get; set; }
        public int Quantity { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}