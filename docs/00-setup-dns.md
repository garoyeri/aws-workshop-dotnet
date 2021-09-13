# Deploying DNS

Checkout the `main` branch first.

> NOTE: in all the `aws` command lines, I use `--profile personal` to indicate the AWS profile I am using. If your profile is `default`, then you don't need to provide this at all. Or if your profile is something else entirely, use that.

## Setup Environment Variables

Copy the `deploy/.env.example` file to `deploy/.env`.

Check your AWS Account number:

```shell
aws sts get-caller-identity --profile personal
```

```json
{
    "UserId": "AIDAVLCCOJS2CWLYZ632O",
    "Account": "999999999999",
    "Arn": "arn:aws:iam::999999999999:user/your-user-name"
}
```

Copy the "Account" value and edit your `.env` file:

```
CDK_DEFAULT_ACCOUNT=999999999999
CDK_DEFAULT_REGION=us-east-1
```

Save this file.

If you have a domain name handy, you can deploy a AWS Route53 Hosted Zone to provide "nice" domain names for your workshop items. In this case, we're using `kcdc.garoyeri.dev` as the subdomain root. Substitute your own domain name if you've got one.

```shell
cd deploy
npm install
dotnet build src
npm run cdk -- bootstrap --profile personal
npm run cdk -- deploy DeployDnsStack --parameters DeployDnsStack:RootDomainName=kcdc.garoyeri.dev --profile personal
```

After the stack deploys (successfully), you'll see a list of outputs:

```
DeployDnsStack.HostedZoneArn = arn:aws:route53:::hostedzone/Z08450421CUAFXM0YLX6M
DeployDnsStack.HostedZoneId = Z08450421CUAFXM0YLX6M
DeployDnsStack.HostedZoneName = kcdc.garoyeri.dev
DeployDnsStack.NameServers = ns-1484.awsdns-57.org,ns-1947.awsdns-51.co.uk,ns-952.awsdns-55.net,ns-64.awsdns-08.com
```

These will be used for setting up your DNS. Go to your DNS provider and add NS records for the desired subdomain. For example on the output above, you should add the following records to your DNS provider:

| Record Type | Record              | TTL    | Value                     |
| ----------- | ------------------- | ------ | ------------------------- |
| `NS`        | `kcdc.garoyeri.dev` | `3600` | `ns-1484.awsdns-57.org`   |
| `NS`        | `kcdc.garoyeri.dev` | `3600` | `ns-1947.awsdns-51.co.uk` |
| `NS`        | `kcdc.garoyeri.dev` | `3600` | `ns-952.awsdns-55.net`    |
| `NS`        | `kcdc.garoyeri.dev` | `3600` | `ns-64.awsdns-08.com`     |

Your DNS provider may represent these accounts in different ways. Usually you'll either see separate entries for each nameserver, or you'll see a single entry with 4 answers. In either case, the DNS entries will be 4 and look like the table above. You can start the TTL with 1 hour for now, then increase it later to 48 hours to reduce the amount of DNS traffic and improve caching.

At this point, your DNS subdomain (`kcdc.garoyeri.dev`) is deferring to AWS to provide information on any further subdomains (such as `hello.kcdc.garoyeri.dev`).

For the workshop, if you don't have a domain name handy, let me know and I'll make a temporary space for you on `your-name.kcdc.garolabs.com`. (I'll keep it running for a month then take it down).