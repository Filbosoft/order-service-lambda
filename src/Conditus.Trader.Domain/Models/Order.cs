using System;
using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;
using Conditus.Trader.Domain.Enums;

namespace Conditus.Trader.Domain.Models
{
    [DynamoDBTable("Orders")]
    public class Order
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        [DynamoDBProperty]
        [DynamoDBRangeKey]
        [Required]
        public string PortfolioId { get; set; }
        [DynamoDBProperty]
        [DynamoDBRangeKey]
        [Required]
        public string CreatedBy { get; set; }
        [DynamoDBProperty]
        [Required]
        public OrderType Type { get; set; }
        [DynamoDBProperty]
        [Required]
        public string AssetId { get; set; }
        [DynamoDBProperty]
        [Required]
        public AssetType AssetType { get; set; }
        [DynamoDBProperty]
        [Required]
        public int Quantity { get; set; }
        [DynamoDBProperty]
        public OrderStatus Status { get; set; }
        [DynamoDBProperty]
        [Required]
        public decimal Price { get; set; }
        [DynamoDBProperty]
        [Required]
        public string Currency { get; set; }
        [DynamoDBProperty]
        [Required]
        public DateTime CreatedAt { get; set; }
        [DynamoDBProperty]
        public DateTime? CompletedAt { get; set; }
        [DynamoDBProperty]
        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}