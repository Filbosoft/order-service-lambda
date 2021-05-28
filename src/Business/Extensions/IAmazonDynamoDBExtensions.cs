using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Business.HelperMethods;
using Conditus.DynamoDBMapper.Mappers;

namespace Business.Extensions
{
    public static class IAmazonDynamoDBExtensions
    {
        public async static Task<T> LoadByLocalIndexAsync<T>(this IAmazonDynamoDB dynamoDB, string hashKey, string rangeKeyName, string rangeKey, string index)
            where T : new()
        {
            string tableName = DynamoDBHelper.GetDynamoDBTableName<T>(),
                hashKeyName = DynamoDBHelper.GetHashKeyName<T>();

            var query = new QueryRequest
            {
                TableName = tableName,
                Select = "ALL_ATTRIBUTES",
                IndexName = index,
                KeyConditionExpression = $"{hashKeyName} = :v_hash_key AND {rangeKeyName} = :v_range_key",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_hash_key", new AttributeValue{S = hashKey}},
                    {":v_range_key", new AttributeValue{S = rangeKey}}
                }
            };

            var response = await dynamoDB.QueryAsync(query);
            var item = response.Items
                .FirstOrDefault();

            if (item == null)
                return default(T);

            var mappedItem = item.ToEntity<T>();
            
            return mappedItem;
        }
    }
}