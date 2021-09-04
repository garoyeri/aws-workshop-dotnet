# Are you there .NET? It's me, AWS

This project is a minimal .NET 5 ASP.NET Core web application that can be deployed on AWS as a Lambda or a Fargate container, or run locally.

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

## Create the Deployment Scripts with CDK

(TODO: add narrative on CDK)

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

