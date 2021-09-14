# Hello World Web + DynamoDB

Checkout the branch `workshop/02-hello-dynamo`.

Use this command to deploy the example from the `deploy` folder:

```shell
npm run cdk -- deploy DeployLambdaStack --profile personal
```

This assumes you've deployed the Lambda in the previous step. If you didn't, you'll need to pass the same `--parameters DeployLambdaStack:DomainName=hello` argument to set the parameter.

This will process any updates to the API Gateway and Lambda, then add the DynamoDB table and appropriate permissions.

## DynamoDB

DynamoDB is a NoSQL database hosted by AWS and secured using AWS IAM. It has some quirks in how you do table design (but that's a completely separate topic and you can start on Twitter here: [@dynamodb](https://twitter.com/dynamodb) where there are a LOT of examples and links to webinars about how to design tables for DynamoDB).

In this case, we're going to extend the simple example (which stores and retrieves values) to store the values in DynamoDB and fetch them.

The API has a few calls:

* `GET /api/values`: get all the values
* `POST /api/values`: add a new value to the next ID
* `PUT /api/values/{id}`: replace the specified ID (or create it)
* `GET /api/values/{id}`: get the specified ID (if it exists)
* `DELETE /api/values/{id}`: delete the specified ID

The table looks something like this:

* Main
  * Hash Key: `id` (example: `value|123`): This is a concatenation so we can store different item types in the same table. Otherwise it's the ID of the value.
  * Attribute: `value` (example: `The String Value`): The value that is set for the ID.
  * Attribute: `dummy` (example: `1`): A value that is used to trick DynamoDB into sorting IDs when we get the secondary index.
* Secondary Index: `SortedIndex`
  * Hash Key: `dummy` (same value for all items)
  * Range Key: `id` (sorted lexographically)
* Other Special Considerations
  * It's a little weird finding the maximum ID (and it is extremely error prone). We handle this by storing a special ID item in the table: `latest|0` that we examine and replace with an incremented index. The latest ID will always be in an item with the Hash Key: `latest|0` and the `value` will be the ID of the latest item. This means that when we do a `POST /api/values`, we'll calculate the latest item and do a batch write to write both changes at once. This will not scale, of course, and if enough people hit the API at once, it will create collisions. I will leave resolving that as an exercise to the inquisitive reader (and also urge people not to create an API with strictly incrementing integral IDs when they know that DynamoDB is in the background.
    * Also note: this is the most complex part of the [`ValuesService`](src/AwsHelloWorldWeb/Features/Values/ValuesService.cs) which is a hint that we're doing it wrong. If you follow good design principles that work WITH DynamoDB instead of against it, you'll find that your interactions with DynamoDB are very short and sweet.
  * DynamoDB will find items by the Hash Key / Range Key combination and perform upserts automatically. So this is extremely simple code.

Starting from the `workshop/02-hello-dynamo` tag, let's see what's looking different.

### Settings

There are some new settings that were added to the [`appsettings.json`](src/AwsHelloWorldWeb/appsettings.json) file that get overridden in Development mode.

```json
  "DynamoDB": {
    "TableNamePrefix": "HelloWorldWeb"
  }
```

Similarly, [`appsettings.Development.json`](src/AwsHelloWorldWeb/appsettings.Development.json) has one more property:

```json
  "DynamoDB": {
    "ServiceURL": "http://localhost:8000",
    "TableNamePrefix": ""
  }
```

`ServiceURL` is used to tell the DynamoDB client to override the endpoint for DynamoDB. By default, the client will look for credentials in the IAM role of the running code as well as the AWS Region the code is running in to select the correct endpoint. Usually, you want to use the endpoint in your current region to read the database (this is faster and cheaper). There is a feature called [DynamoDB Global Tables](https://aws.amazon.com/dynamodb/global-tables/) in case you're interested in building cross-region applications on DynamoDB. In this case, we've used a local URL that will point to the DynamoDB emulator running in a container.

### DynamoDB Local Instance

To make testing easier, there's a Docker Compose project setup in [`test/hello-dynamo/docker-compose.yml`](test/hello-dynamo/docker-compose.yml) that can be spun up with:

```shell
docker compose up -d
```

This will start the local DynamoDB instance and use port 8000 on your local machine to accept connections. When DynamoDB is run locally, it still requires some credentials to be used for the commands, but it will accept any credentials.

During startup, the application will look for the overridden ServiceURL and pass the dummy credentials instead. Otherwise, it will use whatever credentials are part of the IAM Role for the running application in AWS.

AWS has a tool called [NoSQL Workbench for DynamoDB](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/workbench.html) that can be used to work with the local instance and try out queries and table designs. It has some example designs you can look at as well.

### Dynamo DB Context

The .NET client for DynamoDB has three flavors of APIs you can use to interact with DynamoDB:

1. [Low Level API](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Programming.LowLevelAPI.html): this uses the DynamoDB API directly and gives you the most control over interacting with the tables and using all the built-in features of DynamoDB. If you have some complex table designs that require using some of the deeper features of DynamoDB, you'll need this mode.
2. [Document Model](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DotNetSDKMidLevel.html): this simplifies the DynamoDB API into a model that behaves similarly to a MongoDB driver or other C#-friendly Document database. This is likely the simplest API that doesn't require creating any special classes or attributes.
3. [Object Persistence Mode](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DotNetSDKHighLevel.html) (DynamoDBContext): this requires you to define data transfer objects and decorate them with attributes, giving the context hints on how your queries will work. The DynamoDBContext will create the appropriate mappings to return strongly typed objects instead of generic documents of the low level attribute maps. If your data model is well-defined, this will be the easiest way to go and the least error prone. If your data model is in flux or if your items could have completely different attributes, then this mode will give you problems.

In any case, all the modes are available together, so you can mix and match as you need to. If you are using the DynamoDBContext, you'll need to be allowed to call the `DescribeTable` API on the DynamoDB Table, so be sure it include that in your role policy.

### OpenAPI (Swagger) Documentation

For convenience, a Swagger UI page will be hosted on the API when running locally. This makes it much easier to work locally, and you can import the schema file into Postman if that's more your speed.

When the application is deployed to AWS, the Swagger document and UI pages are not created. This saves quite a bit of time on startup.

### Local Integration Testing

The test project was updated to create a test fixture that will create the necesarry table in a local DynamoDB to run the unit tests against a real database. These are handled by the `test/AwsHelloWorldWeb.Tests/IntegrationFixture.cs` file and are created once per test collection. This will create the DynamoDB schema (deleting the old one) and let you run tests against the database.

To run the application and testing locally, you'll need to fire up the local DynamoDB instance. Navigate to the `test/hello-dynamo` folder with your favorite terminal and run the following:

```shell
docker compose up -d
```

This will start the DynamoDB locally on port 8000. If you need a different port, you can edit the docker compose file to remap it. However, there's some other logic around `http://localhost:8000` so you'll need to search and replace the instances of that in the code with your updated port number. 

It will also create a `data` folder where the PostgreSQL files are persisted so that the data is kept around between database creation and deletion. If you want to clear out the database, just stop the container:

```shell
docker compose down
```

And delete the folder.

You can start and stop the service using the Docker Desktop Dashboard.

### Database Permissions

We're still deploying at a Lambda + API Gateway, but also adding the DynamoDB table that's created. We will use the CDK `Table` item to create the DynamoDB table and the Global Secondary Index we'll need to get a sorted list.

After the table is defined, we need to make sure the Lambda is allowed to query it, hence the:

```csharp
table.Table.GrantFullAccess(lambda.Function);
```

### OK Really Deploy It

Use this command to deploy the example from the `deploy` folder:

```shell
npm run cdk -- deploy DeployLambdaStack --profile personal
```

This assumes you've deployed the Lambda in the previous step. If you didn't, you'll need to pass the same `--parameters DeployLambdaStack:DomainName=hello` argument to set the parameter.

This will process any updates to the API Gateway and Lambda, then add the DynamoDB table and appropriate permissions.

