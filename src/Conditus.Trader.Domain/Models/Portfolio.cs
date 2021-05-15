using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace Conditus.Trader.Domain.Models
{
    [DynamoDBTable("Portfolios")]
    public class Portfolio
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        [Required]
        [DynamoDBProperty]
        public string Name { get; set; }
        [Required]
        [DynamoDBProperty]
        [DynamoDBRangeKey]
        public string OwnerId { get; set; }
        [Required]
        [DynamoDBProperty]
        public string Currency { get; set; }
    }
}