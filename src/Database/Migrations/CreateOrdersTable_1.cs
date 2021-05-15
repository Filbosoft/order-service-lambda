using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Conditus.Trader.Domain.Models;

namespace Database.Migrations
{
    public static class CreateOrdersTable_1
    {
        private const string TABLE_NAME = "Orders";

        public static void Up(IAmazonDynamoDB client)
        {
            var currentTables = client.ListTablesAsync().Result;
            List<string> currentTableNames = currentTables.TableNames;
            if (currentTableNames.Contains(TABLE_NAME)) return;
            Console.WriteLine($"Applying {nameof(CreateOrdersTable_1)}");

            var partitionKey = new KeySchemaElement
            {
                AttributeName = "CreatedBy",
                KeyType = KeyType.HASH //Partition key
            };

            var request = new CreateTableRequest
            {
                TableName = TABLE_NAME,
                AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "CreatedBy",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "PortfolioId",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Status",
                        AttributeType = ScalarAttributeType.N
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Type",
                        AttributeType = ScalarAttributeType.N
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "AssetId",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "CreatedAt",
                        AttributeType = ScalarAttributeType.S
                    }
                },
                KeySchema = new List<KeySchemaElement>()
                {
                    partitionKey,
                    new KeySchemaElement
                    {
                        AttributeName = "CreatedAt",
                        KeyType = KeyType.RANGE
                    }
                },
                LocalSecondaryIndexes = new List<LocalSecondaryIndex>
                {
                    new LocalSecondaryIndex
                    {
                        IndexName = LocalIndexes.OrderIdIndex,
                        KeySchema = new List<KeySchemaElement>
                        {
                            partitionKey,
                            new KeySchemaElement
                            {
                                AttributeName = "Id",
                                KeyType = KeyType.RANGE
                            }
                        },
                        Projection = new Projection{ProjectionType = ProjectionType.ALL}
                    },
                    new LocalSecondaryIndex
                    {
                        IndexName = LocalIndexes.OrderStatusIndex,
                        KeySchema = new List<KeySchemaElement>
                        {
                            partitionKey,
                            new KeySchemaElement
                            {
                                AttributeName = "Status",
                                KeyType = KeyType.RANGE
                            }
                        },
                        Projection = new Projection{ProjectionType = ProjectionType.ALL}
                    },
                    new LocalSecondaryIndex
                    {
                        IndexName = LocalIndexes.OrderTypeIndex,
                        KeySchema = new List<KeySchemaElement>
                        {
                            partitionKey,
                            new KeySchemaElement
                            {
                                AttributeName = "Type",
                                KeyType = KeyType.RANGE
                            }
                        },
                        Projection = new Projection{ProjectionType = ProjectionType.ALL}
                    },
                    new LocalSecondaryIndex
                    {
                        IndexName = LocalIndexes.OrderAssetIndex,
                        KeySchema = new List<KeySchemaElement>
                        {
                            partitionKey,
                            new KeySchemaElement
                            {
                                AttributeName = "AssetId",
                                KeyType = KeyType.RANGE
                            }
                        },
                        Projection = new Projection{ProjectionType = ProjectionType.ALL}
                    },
                    new LocalSecondaryIndex
                    {
                        IndexName = LocalIndexes.OrderPortfolioIndex,
                        KeySchema = new List<KeySchemaElement>
                        {
                            partitionKey,
                            new KeySchemaElement
                            {
                                AttributeName = "PortfolioId",
                                KeyType = KeyType.RANGE
                            }
                        },
                        Projection = new Projection{ProjectionType = ProjectionType.ALL}
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 2
                }
            };

            var response = client.CreateTableAsync(request).Result;
        }
        public static void Down(IAmazonDynamoDB client)
        {
            List<string> currentTableNames = client.ListTablesAsync()
                .Result
                .TableNames;

            if (!currentTableNames.Contains(TABLE_NAME)) return;
            Console.WriteLine($"Removing {nameof(CreateOrdersTable_1)}");

            var response = client.DeleteTableAsync(TABLE_NAME).Result;
        }
    }
}