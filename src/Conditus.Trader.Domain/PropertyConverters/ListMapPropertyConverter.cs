using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace  Conditus.Trader.Domain.PropertyConverters
{
    /***
    * This property converter converts a DynamoDB entry of type Map<string, Map> (DynamoDBTypes: M & S)
    * into a list of the passed type and vice versa.
    * 
    * The document / entry structure should look like the following:
    * {
    *   "objectId1" : {"Id":"objectId1","attribute1Key":"attribute1Value"},
    *   "objectId2" : {"Id":"objectId2","attribute1Key":"attribute1Value"}
    * }
    *
    * The reason it's not in a List (DynamoDBType: L) is because it isn't possible to index inside a List
    * but it is on a map.
    * 
    * Sources of knowledge:
    * - https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBContext.ArbitraryDataMapping.html
    ***/
    public class ListMapPropertyConverter<T> : IPropertyConverter
        where T : new()
    {
        public object FromEntry(DynamoDBEntry entry)
        {
            Document entryDocument = entry as Document;
            var entryMap = entryDocument.ToAttributeMap();
            var subEntryMaps = entryMap
                .Select(x => x.Value.M)
                .ToList();

            var entities = new List<T>();

            foreach (var subEntryMap in subEntryMaps)
            {
                var entity = DynamoDBMapper.MapAttributeMapToEntity<T>(subEntryMap);
                entities.Add(entity);
            }

            return entities;
        }

        public DynamoDBEntry ToEntry(object value)
        {
            throw new NotImplementedException();
        }

        /***
        * For some reason will the this not work, as it won't take a non scalar attribute type
        ***/
        // public DynamoDBEntry ToEntry(object value)
        // {
        //     List<T> entities = value as List<T>;
        //     if (entities == null) throw new ArgumentOutOfRangeException();

        //     var entries = new Dictionary<string, AttributeValue>();

        //     foreach (var entity in entities)
        //     {
        //         var subEntry = DynamoDBMapper.GetAttributeMap(entity);
        //         entries.Add(entity.Id, new AttributeValue { M = subEntry});
        //     }

        //     DynamoDBEntry entry = new Primitive
        //     {
        //         Value = new AttributeValue{M = entries} 
        //     };
        //     return entry;
        // }
    }
}