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
```

You'll also get an extra output there: the lambda for the migration. Since this is a relational database, you'll need to setup the schema.

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
