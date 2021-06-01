using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Conditus.DynamoDBMapper.Mappers;

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

        public static string GetHashKeyName<T>()
        {
            var hashProperty = GetHashKeyProperty<T>();
            
            if (hashProperty == null)
                throw new ArgumentOutOfRangeException("T","No hashkey defined on type");
            
            return hashProperty.Name;
        }

        public static PropertyInfo GetHashKeyProperty<T>()
        {
            var type = typeof(T);
            var hashProperty = type.GetProperties()
                .Where(p => p.GetCustomAttribute(typeof(DynamoDBHashKeyAttribute), false) != null)
                .FirstOrDefault();
            
            if (hashProperty == null)
                throw new ArgumentOutOfRangeException("T","No hashkey defined on type");
            
            return hashProperty;
        }

        public static string GetRangeKeyName<T>()
        {
            var hashProperty = GetRangeKeyProperty<T>();
            
            if (hashProperty == null)
                throw new ArgumentOutOfRangeException("T","No rangekey defined on type");
            
            return hashProperty.Name;
        }

        public static PropertyInfo GetRangeKeyProperty<T>()
        {
            var type = typeof(T);
            var hashProperty = type.GetProperties()
                .Where(p => p.GetCustomAttribute(typeof(DynamoDBRangeKeyAttribute), false) != null)
                .FirstOrDefault();
            
            if (hashProperty == null)
                throw new ArgumentOutOfRangeException("T","No rangekey defined on type");
            
            return hashProperty;
        }
    }
}