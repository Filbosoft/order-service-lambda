using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;
using Conditus.Trader.Domain.Models;
using Conditus.Trader.Domain.PropertyConverters;

namespace Conditus.Trader.Domain.Entities
{
    [DynamoDBTable("Portfolios")]
    public class PortfolioEntity
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        [Required]
        [DynamoDBProperty]
        public string Name { get; set; }
        [Required]
        [DynamoDBRangeKey]
        public string OwnerId { get; set; }
        [Required]
        [DynamoDBProperty]
        public decimal Capital { get; set; }
        [DynamoDBProperty(typeof(ListMapPropertyConverter<PortfolioAsset>))]
        public List<PortfolioAsset> Assets { get; set; } = new List<PortfolioAsset>();
    }
}