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

