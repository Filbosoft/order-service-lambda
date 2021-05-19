using System;
using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;
using Conditus.Trader.Domain.Models;

namespace Conditus.Trader.Domain.Entities
{
    public class ExchangeEntity
    {
        [DynamoDBHashKey]
        [Required]
        public string MIC { get; set; }
        [DynamoDBProperty]
        [Required]
        public string Acronym { get; set; }
        [DynamoDBProperty]
        [Required]
        public string Name { get; set; }
        [DynamoDBProperty]
        [Required]
        public Currency Currency { get; set; }
        [DynamoDBProperty]
        [Required]
        public DateTime OpenTime { get; set; }
        [DynamoDBProperty]
        [Required]
        public DateTime CloseTime { get; set; }
        [DynamoDBProperty]
        [Required]
        public DateTime UpdatedAt { get; set; }
        
    }
}