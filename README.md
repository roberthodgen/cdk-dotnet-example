# CDK .NET Example

> Uses AWS Cloud Development Kit (CDK) to deploy a .NET Core MVC API projec to ECS.

## Helpful commands

* `dotnet build -c Release` compile this app
* `cdk deploy`              deploy this stack to your default AWS account/region
* `cdk diff`                compare deployed stack with current state
* `cdk synth`               emits the synthesized CloudFormation template

## Prerequisites

### 1. GitHub Personal Access Token

Create a Personal Access Token and store in AWS Secrets Manager.

See: https://docs.aws.amazon.com/codepipeline/latest/userguide/GitHub-create-personal-token-CLI.html

Configure the name in `IPieplineStackProps`'s `GitHubSecretName` field.
