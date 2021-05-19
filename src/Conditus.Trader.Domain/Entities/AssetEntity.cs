using System;
using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;
using Conditus.Trader.Domain.Enums;
using Conditus.Trader.Domain.Models;

namespace Conditus.Trader.Domain.Entities
{
    public class AssetEntity
    {
        [DynamoDBHashKey]
        [Required]
        public string Symbol { get; set; }
        [DynamoDBProperty]
        [Required]
        public string Name { get; set; }
        [DynamoDBProperty]
        [Required]
        public AssetType Type { get; set; }
        [DynamoDBProperty]
        [Required]
        public Exchange Exchange { get; set; }
        [DynamoDBProperty]
        [Required]
        public Currency Currency { get; set; }
        [DynamoDBProperty]
        [Required]
        public DateTime LastUpdated { get; set; }
    }
}