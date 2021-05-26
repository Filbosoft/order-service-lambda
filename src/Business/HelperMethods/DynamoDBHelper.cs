using System;
using System.Linq;
using System.Reflection;
using Amazon.DynamoDBv2.DataModel;

namespace Business.HelperMethods
{
    public static class DynamoDBHelper
    {
        public static string GetDynamoDBTableName<T>()
        {
            var type = typeof(T);
            var dynamoDBTableAttribute = type.GetCustomAttribute(typeof(DynamoDBTableAttribute), true) as DynamoDBTableAttribute;
            
            if (dynamoDBTableAttribute == null)
                return null;
            
            return dynamoDBTableAttribute.TableName;
        }

        private static string GetHashKeyName<T>()
        {
            var type = typeof(T);
            var hashProperty = type.GetProperties()
                .Where(p => p.GetCustomAttribute(typeof(DynamoDBHashKeyAttribute), false) != null)
                .FirstOrDefault();
            
            if (hashProperty == null)
                throw new ArgumentOutOfRangeException("T","No hashkey defined on type");
            
            return hashProperty.Name;
        }
    }
}