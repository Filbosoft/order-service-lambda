# Development

## Environment
Dependencies/Prerequisites:
- Docker
- Docker compose
- AWS CLI
- .NET 3.1

### Setup

#### Local db
To setup local DynamoDB databases for testing and development run:
1. Start containers: `docker-compose -f .development/localdevelopment.yaml up -d`
2. Migrate databases:
    - `dotnet run -p src/Database/ up --local`
    - `dotnet run -p src/Database/ up --local 9000`

#### Local NuGet feed / Conditus packages
Create a 'nuget.config' file with a source pointing to your local nuget feed.
Ex:
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local-feed" value="/home/{username}/local-nuget-feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

To get the Conditus packages into the local feed:
1. Pull the repo of the nuget package you want
2. Run `dotnet pack`
3. Run `dotnet nuget push -s {Path/To/Local/Feed} {Path/To/NuGetPackage (found in bin/Debug)}`

## Generate user tokens
`aws cognito-idp initiate-auth --auth-flow USER_PASSWORD_AUTH --client-id {clientId}  --auth-parameters USERNAME={username},PASSWORD="{password}"`
aws cognito-idp initiate-auth --auth-flow USER_PASSWORD_AUTH --client-id 7omub5as1em39dsad6r4v9coe8 --auth-parameters USERNAME=TestUser,PASSWORD="Passw0rd#"

- https://stackoverflow.com/questions/49063292/how-to-generate-access-token-for-an-aws-cognito-user 

## Deploying
`dotnet lambda deploy-serverless conditus-trader-order-service --s3-bucket conditus-trader -t src/Api/serverless.template --s3-prefix order-service`

- https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/lambda-cli-publish.html