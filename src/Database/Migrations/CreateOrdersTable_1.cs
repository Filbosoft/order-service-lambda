using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Conditus.Trader.Domain.Entities.LocalSecondaryIndexes;

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
                AttributeName = "OwnerId",
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
                        AttributeName = "OwnerId",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "PortfolioId",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "OrderStatus",
                        AttributeType = ScalarAttributeType.N
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "OrderStatusCreatedAtCompositeKey",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "OrderType",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "AssetSymbol",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "CreatedAt",
                        AttributeType = ScalarAttributeType.N
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
                        IndexName = OrderLocalSecondaryIndexes.UserOrderIdIndex,
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
                        IndexName = OrderLocalSecondaryIndexes.UserOrderStatusIndex,
                        KeySchema = new List<KeySchemaElement>
                        {
                            partitionKey,
                            new KeySchemaElement
                            {
                                AttributeName = "OrderStatusCreatedAtCompositeKey",
                                KeyType = KeyType.RANGE
                            }
                        },
                        Projection = new Projection{ProjectionType = ProjectionType.ALL}
                    },
                    new LocalSecondaryIndex
                    {
                        IndexName = OrderLocalSecondaryIndexes.UserOrderTypeIndex,
                        KeySchema = new List<KeySchemaElement>
                        {
                            partitionKey,
                            new KeySchemaElement
                            {
                                AttributeName = "OrderType",
                                KeyType = KeyType.RANGE
                            }
                        },
                        Projection = new Projection{ProjectionType = ProjectionType.ALL}
                    },
                    new LocalSecondaryIndex
                    {
                        IndexName = OrderLocalSecondaryIndexes.UserOrderAssetIndex,
                        KeySchema = new List<KeySchemaElement>
                        {
                            partitionKey,
                            new KeySchemaElement
                            {
                                AttributeName = "AssetSymbol",
                                KeyType = KeyType.RANGE
                            }
                        },
                        Projection = new Projection{ProjectionType = ProjectionType.ALL}
                    },
                    new LocalSecondaryIndex
                    {
                        IndexName = OrderLocalSecondaryIndexes.UserOrderPortfolioIndex,
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
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new GlobalSecondaryIndex
                    {
                        IndexName = OrderGlobalSecondaryIndexes.OrderStatusIndex,
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement
                            {
                                AttributeName = "OrderStatus",
                                KeyType = KeyType.HASH
                            },
                            new KeySchemaElement
                            {
                                AttributeName = "CreatedAt",
                                KeyType = KeyType.RANGE
                            }
                        },
                        ProvisionedThroughput = new ProvisionedThroughput
                        {
                            ReadCapacityUnits = 10,
                            WriteCapacityUnits = 2
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