## 13/05/2021 [PB] - Initialization
First and foremost this is the commands used to initialize this project:

1. Installed the Lambda templates:
dotnet tool install -g Amazon.Lambda.Tools

2. Used the Serverless template to generate the base application:
dotnet new serverless.AspNetCoreWebAPI

3. Then renamed order-service-lambda API to API

4. Added the projects to a solution file, for better debugging and development purposes:
dotnet new sln
dotnet sln order-service-lambda.sln add src/API test/API.Tests

## 15/05/2021 [PB] - Test setup
Today I will setup the acceptance tests for the API.
Previously I've tried to use the TestLambdaContext and the LambdaEntryPoint, but there seems to be a bug that never shutdown the host.
This bug means that running tests will sometimes throw an error: "Too many instances", which is definitely not ideal.
My idea to prevent this is to use standard integration testing, as the API can communicate as a standard API it will be able to check all the logic. The down side is that the test requests won't have the lambda request format, but as this is something that the API Gateway will facilitate in the system anyways I don't find it necessary to specify here.

So to sum up the plan: Make use of the WebApplicationFactory as used when integration testing "normal" .NET APIs, and test if it's sufficient for it's purpose.

Update #1:
It seems to work for simple tests with no dependencies. It correctly shutdown the test server after test.

## 19/05/2021 [PB] - Create & read order functionality
Today I will start implementing create and read of orders.  

Steps:
- Create acceptance tests for create
- Determine if there's a need for layered architecture.

**Decision: Layered architecture**  
This will be a micro service and should therefore be as simple as possible, but there's still quite a lot of functionality which needs to be implemented, and say we wish to implement graphql in the future, it would be nice to have a separate business layer at least. The data access layer won't make much sense as the business logic is how to query and insert data in the database, and will therefore be in the business layer.  
But accessing the portfolio and asset services should probably go through a DAL, so a DAL layer might be useful just for handling dependencies to other services.

**Conclusion:** The service will include a presentation layer in form of a rest API, a business layer including db requests and a DAL to handle communication with the other services.