using System;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Conditus.Trader.Domain.PropertyConverters
{
    public class UTCDateTimePropertyConverter : IPropertyConverter
    {
        public object FromEntry(DynamoDBEntry entry)
        {
            var primitive = entry as Primitive;

            if (primitive == null 
                || !(primitive.Value is string)
                || string.IsNullOrEmpty((string)primitive.Value))
                throw new ArgumentOutOfRangeException();

            return Convert.ToDateTime(primitive.Value as string);
        }

        public DynamoDBEntry ToEntry(object value)
        {
            if (value == null) 
                return new Primitive { Value = null};
            
            var stringValue = value.ToString();

            return new Primitive
            {
                Value = stringValue
            };
        }
    }
}