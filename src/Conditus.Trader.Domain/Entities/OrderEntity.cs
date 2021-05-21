using System;
using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.PropertyConverters;

namespace Conditus.Trader.Domain.Entities
{
    [DynamoDBTable("Orders")]
    public class OrderEntity
    {
        [DynamoDBProperty]
        public string Id { get; set; }
        [DynamoDBProperty]
        [Required]
        public string PortfolioId { get; set; }
        [DynamoDBHashKey]
        [Required]
        public string CreatedBy { get; set; }
        [DynamoDBProperty]
        [Required]
        public OrderType OrderType { get; set; } //Type is a keyword in dynamodb and can therefore not be used in expressions
        [DynamoDBProperty]
        [Required]
        public string AssetSymbol { get; set; }
        [DynamoDBProperty]
        public AssetType AssetType { get; set; }
        [DynamoDBProperty]
        [Required]
        public string AssetName { get; set; }
        [DynamoDBProperty]
        [Required]
        public int Quantity { get; set; }
        [DynamoDBProperty] 
        public OrderStatus OrderStatus { get; set; } //Status is a keyword in dynamodb and can therefore not be used in expressions
        [DynamoDBProperty]
        [Required]
        public decimal Price { get; set; }
        [DynamoDBRangeKey(typeof(DateTimePropertyConverter))]
        [Required]
        public DateTime CreatedAt { get; set; }
        [DynamoDBProperty(typeof(DateTimePropertyConverter))]
        public DateTime? CompletedAt { get; set; }
        [DynamoDBProperty(typeof(DateTimePropertyConverter))]
        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}