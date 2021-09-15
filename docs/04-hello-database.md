# Relational Database Madness

The next part is a bit harder than it should be, but it still follows the pattern that AWS is "secure by default" and you need to do things the "right way".

Checkout the branch `main`. We need to deploy the new application instance and the database.

```shell
npm run cdk -- deploy DeployDatabaseContainerStack --parameters DeployContainerStack:DomainName=hello-database --profile personal
```

This will deploy an application and the outputs will include the DNS name and the Load Balancer URL (again, in case you didn't setup a domain name):

```
Outputs:
DeployDatabaseContainerStack.HelloContainerServiceLoadBalancerDNS8D6004C8 = Deplo-Hello-P15QD5D8MZ50-660410385.us-east-1.elb.amazonaws.com
DeployDatabaseContainerStack.HelloContainerServiceServiceURLBB0F4736 = https://hello-database.kcdc.garoyeri.dev
DeployDatabaseContainerStack.MigrationFunctionArn = arn:aws:lambda:us-east-1:367359052980:function:DeployDatabaseContainerSta-MigrationLambda8A60C9F2-JwyC3u954zYe
```

You'll also get an extra output there: the lambda for the migration. Since this is a relational database, you'll need to setup the schema. Run the following command next:

```shell
aws lambda invoke --function-name arn:aws:lambda:us-east-1:367359052980:function:DeployDatabaseContainerSta-MigrationLambda8A60C9F2-JwyC3u954zYe response.txt --profile personal
```

Replace the ARN with the one from your own output. This will execute the migration and you'll get an OK response that it worked out. Then access the page: <https://hello-database.kcdc.garoyeri.dev>.

## Running Locally

To try it out locally, setup the local PostgreSQL container database. Navigate to the `test/hello-postgresql` folder in your favorite terminal, and run the following:

```shell
docker compose up -d
```

This will fire up a clean PostgreSQL database on port 5432. It will also create a `data` folder where the PostgreSQL files are persisted so that the data is kept around between database creation and deletion. If you want to clear out the database, just stop the container:

```shell
docker compose down
```

And delete the folder.

You can start and stop the service using the Docker Desktop Dashboard.

### Switch the Application to "Database" Mode

Edit the `src/AwsHelloWorldWeb/appsettings.json` file and change the `PersistenceMode` setting to `"Database"`:

```json
"Database": {
  "PersistenceMode": "Database"
}
```

This will make sure all the relational database configuration gets turned on.

### Run Database Migrations

Before you can run the application for first time, you'll need to make sure the .NET tools are installed. From the same folder as the main solution file (the root of the project), run the following command:

```shell
dotnet tool install
```

This will install the EF Core CLI tool. You'll only need to do this once to get the tool installed locally.

Run the following command to setup the database:

```shell
dotnet ef database update
```

If everything is setup correctly, it should create a table in the database.

## Secrets

You may have noticed that there was no password creation or handoff of credentials for the database. If you scour the CDK, you won't see a hardcoded password string anywhere. So what gives? Well, the CDK will generate a randomized secret password, and the database construct will populate the rest. The secret in AWS looks suspiciously like the `src/AwsHelloWorldWeb/DatabaseSettings.cs` file, and gets populated at startup.

I've added the code from the [`Kralizek.Extensions.Configuration.AWSSecretsManager`](https://github.com/Kralizek/AWSSecretsManagerConfigurationExtensions) library, which needed an upgrade on its dependencies, but it otherwise unchanged. This magical little library will fetch the specified secrets and populate them in the configuration store.

## Lambda Too!

There is a Lambda + Database deployment stack as well: `DeployDatabaseLambdaStack` if you're interested. It works the same way, except that the API itself is a Lambda instead of a container. The migration function is still a Lambda (because that's easier).
