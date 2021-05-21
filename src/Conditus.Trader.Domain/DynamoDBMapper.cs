using System;
using System.Collections;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace Conditus.Trader.Domain
{
    public static class DynamoDBMapper
    {
        public static Dictionary<string, AttributeValue> GetAttributeMap(object entity)
        {
            var map = new Dictionary<string, AttributeValue>();

            foreach (var property in entity.GetType().GetProperties())
            {
                var propertyType = property.PropertyType;
                var propertyValue = property.GetValue(entity);
                if (propertyValue == null) continue;

                AttributeValue mapValue;

                if (propertyType == typeof(decimal) || propertyType == typeof(int) || propertyType == typeof(long))
                    mapValue = new AttributeValue { N = propertyValue.ToString() };
                else if (propertyType.IsEnum)
                    mapValue = new AttributeValue { N = ((int)propertyValue).ToString() };
                else if (propertyType == typeof(string))
                    mapValue = new AttributeValue { S = (string)propertyValue };
                else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                    mapValue = new AttributeValue { S = propertyValue.ToString() };
                // else if (propertyValue is IEnumerable)
                //     mapValue = new AttributeValue{ M = ListToMap((IEnumerable<object>) propertyValue)};
                else
                    mapValue = new AttributeValue { M = GetAttributeMap(propertyValue) };

                map.Add(property.Name, mapValue);
            }

            return map;
        }

        // private static Dictionary<string, AttributeValue> ListToMap(IEnumerable<object> entites)
        // {
        //     var map = new Dictionary<string, AttributeValue>();

        //     foreach (var entity in entites)
        //     {
        //         var id = ((BaseModel) entity).Id;
        //         var attributeMap = GetAttributeMap(entity);

        //         map.Add(id, new AttributeValue{M = attributeMap});
        //     }

        //     return map;
        // }

        public static T MapAttributeMapToEntity<T>(Dictionary<string, AttributeValue> attributeMap)
            where T : new()
        {
            var entity = new T();

            foreach (var property in entity.GetType().GetProperties())
            {
                var entityProperty = entity.GetType().GetProperty(property.Name);
                if (entityProperty == null || !entityProperty.CanWrite) continue;

                var attributeValue = attributeMap.GetAttributeValue(property.Name);
                if (attributeValue == null) continue;

                object propertyValue;

                var propertyType = property.PropertyType;

                if (propertyType == typeof(decimal))
                    propertyValue = Convert.ToDecimal(attributeValue.N);
                else if (propertyType == typeof(int))
                    propertyValue = Convert.ToInt32(attributeValue.N);
                else if (propertyType.IsEnum)
                    propertyValue = Enum.ToObject(propertyType, int.Parse(attributeValue.N));
                else if (propertyType == typeof(string))
                    propertyValue = attributeValue.S;
                else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                    propertyValue = GetDateTimeFromUnixTimeMS(attributeValue.N);
                else if (propertyType == typeof(object))
                    propertyValue = Convert.ChangeType(MapAttributeMapToEntity<object>(attributeValue.M), propertyType);
                else continue;

                entityProperty.SetValue(entity, propertyValue);
            }

            return entity;
        }

        public static DateTime GetDateTimeFromUnixTimeMS(string unixTime)
        {
            try
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(unixTime));
                return dateTimeOffset.UtcDateTime;
            }
            catch (ArgumentOutOfRangeException)
            {
                return new DateTime();
            }
        }

        public static AttributeValue GetAttributeValue(this DateTime dateTime)
        {
            var unixTime = GetUnixTimeMSFromDateTime(dateTime);
            var value = new AttributeValue { N = unixTime.ToString() };

            return value;
        }

        public static long GetUnixTimeMSFromDateTime(DateTime dateTime)
        {
            var offset = new DateTimeOffset(dateTime, new TimeSpan());
            var unixTime = offset.ToUnixTimeMilliseconds();

            return unixTime;
        }

        public static AttributeValue GetAttributeValue(this Enum enumValue)
        {
            var numericValue = Convert.ToUInt32(enumValue);
            var value = new AttributeValue { N = numericValue.ToString() };

            return value;
        }

        public static AttributeValue GetAttributeValue(this Dictionary<string, AttributeValue> attributeMap, string key)
        {
            AttributeValue attributeValue;
            attributeMap.TryGetValue(key, out attributeValue);

            return attributeValue;
        }
    }
}