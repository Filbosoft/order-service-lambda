using System;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace Conditus.Trader.Domain.PropertyConverters
{
    public class DateTimePropertyConverter : IPropertyConverter
    {
        public object FromEntry(DynamoDBEntry entry)
        {
            var primitive = entry as Primitive;

            if (primitive == null)
                throw new ArgumentOutOfRangeException();
            
            var longValue = long.Parse((string)primitive.Value);
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(longValue);

            return dateTimeOffset.DateTime;
        }

        public DynamoDBEntry ToEntry(object value)
        {
            if (value == null)
                return new Primitive { Value = null, Type = DynamoDBEntryType.Numeric };

            try
            {
                var dateValue = ((DateTime)value).ToUniversalTime();
                var unixTime = new DateTimeOffset(dateValue).ToUnixTimeMilliseconds();
                var stringValue = unixTime.ToString();

                return new Primitive { Value = stringValue, Type = DynamoDBEntryType.Numeric};
            }
            catch (ArgumentOutOfRangeException)
            {
                return new Primitive { Value = null, Type = DynamoDBEntryType.Numeric };
            }

        }
    }
}