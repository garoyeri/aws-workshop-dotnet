# Containerized Application on Fargate

The next step is to explore the containerized application on AWS Fargate. So far, we've deployed Lambas, but the next step is more common for heavier ASP.NET Core applications, and anything that uses Razor pages. The Lambda approach works well for APIs, but not full websites that would benefit from more in-process caching.

Checkout the branch `workshop/03-hello-container`. We need to deploy two things here: the VPC and the container application. Use the following commands:

```shell
npm run cdk -- deploy DeployVpcStack --profile personal
npm run cdk -- deploy DeployContainerStack --parameters DeployContainerStack:DomainName=hello-container --profile personal
```

This will deploy an application and the outputs will include the DNS name and the Load Balancer URL (again, in case you didn't setup a domain name):

```shell
Outputs:
DeployContainerStack.HelloContainerServiceLoadBalancerDNS8D6004C8 = Deplo-Hello-P15QD5D8MZ50-660410385.us-east-1.elb.amazonaws.com
DeployContainerStack.HelloContainerServiceServiceURLBB0F4736 = https://hello-container.kcdc.garoyeri.dev
```

The DynamoDB works exactly the same as in the Lambda example in Workshop 02, the only difference is that the application is now running as a Fargate service instead of a Lambda. Api Gateway + Lambda can spin down when it's not in use, but the Fargate service will be running all the time. You can also extend this with autoscaling rules to allow the service to create a second instance if there are a lot of users and you need to spread the load across availability zones.

## Why the VPC?

We deployed a VPC in this step because the containers require a VPC to connect their network interface to. With Lambda, the network interface is created and shared behind the scenes (managed by AWS), but with the containers, you have to provide a network for it to use and it will manage its own network interfaces.

With DynamoDB, the only requirement is that the container needs to have a way to talk to the public network (the internet) so it can make the API calls against DynamoDB. In the next workshop, we'll talk about relational databases and the VPC will play a more key role.
