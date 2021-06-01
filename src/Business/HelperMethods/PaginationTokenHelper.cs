using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using Conditus.DynamoDBMapper.Mappers;
using Conditus.Trader.Domain.Entities;

namespace Business.HelperMethods
{
    public static class PaginationTokenHelper
    {
        public const char KEY_SEPARATOR = '.';
        
        public static string GetTokenWithRangeKey<TEntity>(Dictionary<string, AttributeValue> lastEvaluatedKey)
            where TEntity : new()
        {
            var hashKeyProperty = DynamoDBHelper.GetHashKeyProperty<TEntity>();
            var rangeKeyProperty = DynamoDBHelper.GetRangeKeyProperty<TEntity>();
            var entity = lastEvaluatedKey.ToEntity<TEntity>();
            var hashKey = hashKeyProperty.GetValue(entity);
            var rangeKey = rangeKeyProperty.GetValue(entity);
            
            var token = $"{hashKey}{KEY_SEPARATOR}{rangeKey}";

            return token;
        }

        public static string GetToken<TEntity>(Dictionary<string, AttributeValue> lastEvaluatedKey)
            where TEntity : new()
        {
            var hashKeyProperty = DynamoDBHelper.GetHashKeyProperty<TEntity>();
            var entity = lastEvaluatedKey.ToEntity<TEntity>();
            var hashKey = hashKeyProperty.GetValue(entity);
            
            var token = $"{hashKey}";

            return token;
        }

        public static Dictionary<string, AttributeValue> GetLastEvaluatedKey<TEntity>(string token)
        {
            var hashKeyProperty = DynamoDBHelper.GetHashKeyProperty<TEntity>();
            var hashKey = Convert.ChangeType(token, hashKeyProperty.PropertyType);
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>
            {
                {hashKeyProperty.Name, hashKey.GetAttributeValue()}
            };

            return lastEvaluatedKey;
        }

        public static Dictionary<string, AttributeValue> GetLastEvaluatedKeyWithRangeKey<TEntity>(string token)
        {
            var tokenParts = token.Split(KEY_SEPARATOR);
            var hashKeyProperty = DynamoDBHelper.GetHashKeyProperty<TEntity>();
            var hashKey = Convert.ChangeType(tokenParts[0], hashKeyProperty.PropertyType);

            var rangeKeyProperty = DynamoDBHelper.GetRangeKeyProperty<TEntity>();
            var rangeKey = Convert.ChangeType(tokenParts[1], rangeKeyProperty.PropertyType);
            
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>
            {
                {hashKeyProperty.Name, hashKey.GetAttributeValue(hashKeyProperty.PropertyType)},
                {rangeKeyProperty.Name, rangeKey.GetAttributeValue(rangeKeyProperty.PropertyType)}
            };

            return lastEvaluatedKey;
        }
    }
}