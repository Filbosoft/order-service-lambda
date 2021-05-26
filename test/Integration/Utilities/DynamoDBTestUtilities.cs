using System;

namespace Integration.Utilities
{
    public static class DynamoDBTestUtilities
    {
        /***
        * As DynamoDB doesn't have a native DateTime data type.
        * DateTimes are stored as strings, meaning the format will alter some of the detail
        * resulting in comparisons being false, even though the time is the same.
        * 
        * To avoid this, this function formats the DateTime to the same format as in the DB.
        ***/
        public static DateTime ToDbDateTime(this DateTime dateTime)
        {
            return Convert.ToDateTime(dateTime.ToString());
        }
    }
}