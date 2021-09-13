# Are you there .NET? It's me, AWS.

This project is a minimal .NET 5 ASP.NET Core web application that can be deployed on AWS as a Lambda or a Fargate container, or run locally.

[TOC]

## Building this repository from scratch

This set of commands will setup the solution, add the new projects, configure some sensible defaults, and open it up in Visual Studio Code. At this point, you could easily open it in any desired IDE: Visual Studio, Rider, Code, whatever.

```shell
dotnet new -i Amazon.Lambda.Templates

mkdir AwsHelloWorldWeb
cd AwsHelloWorldWeb
dotnet new gitignore
dotnet new tool-manifest
dotnet tool install amazon.lambda.tools
dotnet new sln
dotnet new serverless.image.AspNetCoreWebAPI HelloWeb
dotnet sln add src/**
dotnet sln add test/**
dotnet build
code .
```

At this point, you can run the web application like a normal ASP.NET Core application. Go ahead, try it!

After this point, I've made modifications, so if you want to see exactly what those changes were, you can check the difference between what you generate locally and what the code looks like.

## Create the Deployment Scripts with CDK

[AWS Cloud Development Kit (CDK)](https://aws.amazon.com/cdk/) is a deployment technology from AWS that makes it easier to build up CloudFormation templates for deploying your code. You write your declarative deployment structures in a comfortable language such as TypeScript, C#, Python, or Java instead of creating them directly in YAML. CDK has an opionated approach on deploying into AWS that sets up a reasonably happy path as long as you stay near it. It also has a CloudFormation escape hatch where you can manually create CloudFormation constructs or create your own higher level constructs to do things the way you want.

The following commands will generate the deployment project template:

```shell
mkdir deploy
cd deploy
npx aws-cdk@1.121.0 init app -l csharp
npm init -y
npm install aws-cdk@1.121.0 --save-exact
```

At this point, edit the `package.json` file to replace the `scripts` section to allow us to run the `cdk` tool. If you want, change the license as well. The whole file will look like this:

```json
{
  "name": "deploy",
  "version": "1.0.0",
  "description": "This is a blank project for C# development with CDK.",
  "main": "index.js",
  "scripts": {
    "cdk": "cdk"
  },
  "keywords": [],
  "author": "",
  "license": "MIT",
  "dependencies": {
    "aws-cdk": "1.121.0"
  }
}
```

Next we'll build the code and try out generating a blank AWS to make sure it works:

```shell
dotnet build src
npm run cdk -- synth
```

You should see some YAML that represents the minimal empty CDK configuration.

Let's also add this deployment project to the parent solution:

```shel
cd ..
dotnet sln add deploy/src/Deploy
dotnet build
```

### A disclaimer on CDK versioning

As you work with CDK, you'll pull new package references into your project to add support for other AWS services. Be very careful to keep the CDK version numbers exactly the same for all packages. The project file: `deploy/src/Deploy/Deploy.csproj` defines a property and a property group to make it easier to update all the versions if you decide to upgrade CDK to resolve issues or support newer services:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <!-- Roll forward to future major versions of the netcoreapp as needed -->
    <RollForward>Major</RollForward>
  </PropertyGroup>

  <PropertyGroup>
    <CDKVersion>1.121.0</CDKVersion>
  </PropertyGroup>

  <ItemGroup>
    <!-- CDK Construct Library dependencies -->
    <PackageReference Include="Amazon.CDK" Version="$(CDKVersion)" />
    <PackageReference Include="Amazon.CDK.AWS.APIGatewayv2" Version="$(CDKVersion)" />
    <PackageReference Include="Amazon.CDK.AWS.APIGatewayv2.Integrations" Version="$(CDKVersion)" />
    <PackageReference Include="Amazon.CDK.AWS.ECR" Version="$(CDKVersion)" />
    <PackageReference Include="Amazon.CDK.AWS.CertificateManager" Version="$(CDKVersion)" />
    <PackageReference Include="Amazon.CDK.ECR.Assets" Version="$(CDKVersion)" />
    <PackageReference Include="Amazon.CDK.AWS.Lambda" Version="$(CDKVersion)" />
    <PackageReference Include="Amazon.CDK.AWS.Route53" Version="$(CDKVersion)" />
    <PackageReference Include="Amazon.CDK.AWS.Route53.Targets" Version="$(CDKVersion)" />

    <!-- jsii Roslyn analyzers (un-comment to obtain compile-time checks for missing required props -->
    <PackageReference Include="Amazon.Jsii.Analyzers" Version="*" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

This way, you can update the `<CDKVersion>` element once to update all the package references and keep them in lock step.

There's little stopping you from staying with a particular version. CDK is based on CloudFormation whose template schema version is still dated `2010-09-09`. All the AWS services likewise tend to be very backwards compatible, so there's little chance of something suddenly completely stopping working. The only reason to upgrade would be to support bug fixes or if there's a new service or setting that is available only in new configuration schemas for those services.

## Setup AWS CLI

The rest of the instructions assume that you've got an AWS account setup already, you've setup a programmatic access user, and created a local CLI profile called `"personal`" with the access key an secret key.

## Workshop 00: Deploying DNS

To make it easier to find the applications that are being deployed, we need to setup DNS so we can have nice names. This is very important because "naming things" is one of the hardest problems in software development.

Go to [Deploying DNS](docs/00-setup-dns.md).

## Workshop 01: Deploying the Hello World Web

Next, we'll deploy a Lambda + ASP.NET Core web application.

Go to [Deploying the Hello World Web](docs/01-hello-lambda.md).

## Workshop 02: Adding DynamoDB

Next, we'll deploy a DynamoDB table as well and persist the values in the API.

Go to [Adding DynamoDB](docs/02-hello-dynamo.md).

## Workshop 03: Containerized Application on Fargate

Go to [Containerized Application on Fargate](docs/03-hello-container.md).

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



