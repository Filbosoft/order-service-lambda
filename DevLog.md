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
