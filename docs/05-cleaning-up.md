# Cleaning up Everything

AWS doesn't do charity, so if you leave resources running, you'll be billed for them. Let's systematically go through and clean up everything we built here.

While you can use the CDK command line to delete it, I recommend that here because it's faster just to delete the stacks in CloudFormation. Access the CloudFormation service from your AWS account Console, You should see a number of stacks that start with the word "Deploy":

* CdkBootstrap
* DeployDnsStack
* DeployVpcStack
* DeployLambdaStack
* DeployContainerStack
* DeployDatabaseLambdaStack (if you tried this on your own)
* DeployDatabaseContainerStack

You can start deleting multiple stacks at once. Select the last 4 stacks in that list and delete them. This process will take some time as it goes and cleans up resources.

Once completed, then you can safely first delete the DeployVpcStack, then delete the DeployDnsStack. AWS charges $0.50 per month per hosted zone, and the VPC has a NAT Gateway that will bill you around $30 if it runs all month long.

When deleting the DeployDnsStack, it may get stuck due to extra entries that were added during the SSL cerrtificate validation. Access the Route 53 service on the AWS Console and delete the "weird" `TXT` entry that definitely looks like it was made by a machine.

The CdkBootstrap stack is only storage. It won't delete correctly unless you go and find the S3 Bucket and empty it, then run the Stack Delete.

## Manual Cleanup

Some things might still be hanging around, but let's check.

1. DynamoDB: ensure all the tables are deleted
2. CloudWatch: ensure all the Log Groups are deleted
3. RDS: ensure all the databases are deleted
   1. Snapshots: check for any database snapshots you created here and delete them

Afterwards, wait 48 hours and check the Cost Explorer on your AWS Account to be sure that there aren't any more charges being billed.