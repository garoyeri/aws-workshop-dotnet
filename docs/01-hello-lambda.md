# Deploying the Hello World Web (Lambda)

Next, we'll deploy the Hello World Web. Checkout the branch `workshop/01-hello-world-web`.

If you're not using a custom domain name, then you need to edit [`deploy/src/Deploy/Program.cs`](deploy/src/Deploy/Program.cs) and change the `skipCertificate: false` to `skipCertificate: true` on the  statement containing `new DeployLambdaStack`. Otherwise the deployment will fail trying to validate the certificate.

Deploy using the following command:

```shell
npm run cdk -- deploy DeployLambdaStack --parameters DeployLambdaStack:DomainName=hello --profile personal
```

The `DeployLambdaStack:DomainName=hello` entry is the subdomain from the root domain you want to use. The stacks use values from each other so you don't have to re-enter items that don't need to be re-entered.

When you run this, it will ask you to confirm changes to IAM. These are the security settings that are created automatically by CDK when you link things together. It will configure the correct permissions for you automatically, but any changes to IAM will need to be reviewed on a deployment. If you redeploy and there are no IAM changes, then this confirmation will not appear.

The outputs will include a URL that can be used instead of the custom domain name you added. So if you chose not to apply a custom domain, then use this URL. For example:

```shell
Outputs:
DeployLambdaStack.ApiEndpoint = https://93v78n62g7.execute-api.us-east-1.amazonaws.com
```

If you decided to use a custom URL like mine (https://hello.kcdc.garoyeri.dev), then that's the base URL.

Let's try one and see what happens, point your browsers to: <https://hello.kcdc.garoyeri.dev> and see what happens.

![image-20210906180727544](/Users/garo.yeriazarian/Documents/headspring/AwsHelloWorldWeb/docs/images/Hello-World-Running.png)

If you get a "Connection Refused", make sure you're using the HTTPS URL and not HTTP!