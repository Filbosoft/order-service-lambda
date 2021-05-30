using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Database.Migrations;

namespace Database
{
    class Program
    {
        /***
        * This system was made to keep track of the changes to the database for the order-service-lambda.
        * Use:
        * - To migrate your local instance of DynamoDB run: dotnet run up --local [{localEndpoint}]
        *   Note that the localEndpoint will default to http://localhost:8000 if not provided.
        * - To remove the migration run the same command with "up" replaced by "down"
        ***/
        static void Main(string[] args)
        {
            var isValidRequest = ValidateRequest(args);
            if (!isValidRequest) return;

            Console.WriteLine("Migrating order service data store");
            var arguments = args.ToList();
            var client = CreateClient(arguments);

            var upOrDown = arguments[0];
            if (upOrDown.ToUpper().Equals("UP")) Up(client);
            else if (upOrDown.ToUpper().Equals("DOWN")) Down(client);
            Console.WriteLine("Migration complete");
        }

        private static bool ValidateRequest(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No parameters specified, please provide the type of migration (up|down)");
                return false;
            }

            var firstArg = args[0].ToUpper();
            if (!firstArg.Equals("UP") && !firstArg.Equals("DOWN"))
            {
                Console.WriteLine("No up or down parameter specified");
                return false;
            }

            return true;

        }

        private static IAmazonDynamoDB CreateClient(List<string> args)
        {
            var clientConfig = new AmazonDynamoDBConfig();
            var indexOfLocal = args.IndexOf("--local");

            if (indexOfLocal > -1)
            {

                string followingArg = null;
                try
                {
                    followingArg = args[indexOfLocal + 1];
                }
                catch (ArgumentOutOfRangeException)
                { }

                var servicePort = followingArg != null ? followingArg : "8000";
                var serviceURL = "http://localhost:" + servicePort;

                clientConfig.ServiceURL = serviceURL;
            }
            else
            {
                bool answered = false;
                var answer = "";
                while (!answered)
                {
                    Console.Write("Are you sure you want to migrate the production db? (Y|N): ");
                    answer = Console.ReadLine();

                    if (answer.ToUpper().Equals("Y") 
                        || answer.ToUpper().Equals("N"))
                        answered = true;
                    else
                        Console.WriteLine("Invalid input, must be Y or N");
                }

                if (answer.ToUpper().Equals("N")) throw new Exception("Request cancelled by user");
            }

            return new AmazonDynamoDBClient(clientConfig);
        }

        static void Up(IAmazonDynamoDB client)
        {
            CreateOrdersTable_1.Up(client);
        }

        static void Down(IAmazonDynamoDB client)
        {
            CreateOrdersTable_1.Down(client);
        }
    }
}
