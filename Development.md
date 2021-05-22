# Development

## Environment
Dependencies/Prerequisites:
- Docker
- Docker compose
- AWS CLI
- .NET 3.1

### Setup
To setup local DynamoDB databases for testing and development run: `docker-compose -f .development/localdevelopment.yaml up -d`

## Generate user tokens
`aws cognito-idp initiate-auth --auth-flow USER_PASSWORD_AUTH --client-id {clientId}  --auth-parameters USERNAME={username},PASSWORD="{password}"`

- https://stackoverflow.com/questions/49063292/how-to-generate-access-token-for-an-aws-cognito-user 

## Deploying
`dotnet lambda deploy-serverless conditus-trader-order-service --s3-bucket conditus-trader -t src/API/serverless.template --s3-prefix order-service`

- https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/lambda-cli-publish.html 