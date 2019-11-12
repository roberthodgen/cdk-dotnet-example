namespace Infrastructure.Stacks
{
    using System.Collections.Generic;
    using Amazon.CDK;
    using Amazon.CDK.AWS.CodeBuild;
    using Amazon.CDK.AWS.CodePipeline;
    using Amazon.CDK.AWS.CodePipeline.Actions;
    using Amazon.CDK.AWS.ECR;
    using Amazon.CDK.AWS.IAM;
    using Props;
    using Util;

    public class PipelineStack : Stack
    {
        public IRepository EcrRepository { get; }

        public PipelineStack(Construct parent, string id, IPipelineStackProps props) : base(parent, id, props)
        {
            EcrRepository = new Repository(
                this,
                "EcrRepository",
                new RepositoryProps
                {
                    RepositoryName = "cdk-dotnet-example",
                    RemovalPolicy = RemovalPolicy.DESTROY,
                });

            var cdkBuild = new PipelineProject(
                this,
                "CdkBuild",
                new PipelineProjectProps
                {
                    BuildSpec = BuildSpec.FromSourceFilename("Infrastructure/Resources/cdk_buildspec.yml"),
                    Environment = new BuildEnvironment
                    {
                        BuildImage = LinuxBuildImage.AMAZON_LINUX_2,
                    },
                });

            var apiBuildTaskRole = new Role(
                this,
                "ApiBuildRole",
                new RoleProps
                {
                    ManagedPolicies = new[]
                    {
                        ManagedPolicy.FromAwsManagedPolicyName("AmazonEC2ContainerRegistryPowerUser"),
                    },
                    AssumedBy = new ServicePrincipal("codebuild.amazonaws.com")
                });

            var apiBuild = new PipelineProject(
                this,
                "ApiBuild",
                new PipelineProjectProps
                {
                    BuildSpec = BuildSpec.FromSourceFilename("Infrastructure/Resources/api_buildspec.yml"),
                    Role = apiBuildTaskRole,
                    Environment = new BuildEnvironment
                    {
                        BuildImage = LinuxBuildImage.AMAZON_LINUX_2,
                        Privileged = true,
                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                        {
                            ["AWS_ACCOUNT_ID"] = CdkUtil.PlainTextBuildEnvironmentVariable(Of(this).Account),
                            ["AWS_DEFAULT_REGION"] = CdkUtil.PlainTextBuildEnvironmentVariable(Of(this).Region),
                        },
                    },
                });

            var apiTest = new PipelineProject(
                this,
                "ApiTest",
                new PipelineProjectProps
                {
                    BuildSpec = BuildSpec.FromSourceFilename("Infrastructure/Resources/ci_buildspec.yml"),
                    Environment = new BuildEnvironment
                    {
                        BuildImage = LinuxBuildImage.AMAZON_LINUX_2,
                        Privileged = true,
                    },
                });

            var sourceOutput = new Artifact_();
            var cdkBuildOutput = new Artifact_("CdkBuildOutput");
            var apiBuildOutput = new Artifact_("ApiBuildOutput");

            new Pipeline(
                this,
                "Api",
                new PipelineProps
                {
                    Stages = new IStageProps[]
                    {
                        new StageProps
                        {
                            StageName = "Source",
                            Actions = new IAction[]
                            {
                                new GitHubSourceAction(new GitHubSourceActionProps
                                {
                                    ActionName = "GitHub",
                                    OauthToken = SecretValue.SecretsManager(props.GitHubSecretName),
                                    Repo = props.GitHubRepo,
                                    Owner = props.GitHubOwner,
                                    Output = sourceOutput,
                                    Trigger = GitHubTrigger.WEBHOOK,
                                }),
                            },
                        },
                        new StageProps
                        {
                            StageName = "Build",
                            Actions = new IAction[]
                            {
                                new CodeBuildAction(new CodeBuildActionProps
                                {
                                    ActionName = "ApiTest",
                                    Project = apiTest,
                                    Input = sourceOutput,
                                    RunOrder = 1,
                                }),
                                new CodeBuildAction(new CodeBuildActionProps
                                {
                                    ActionName = "ApiBuild",
                                    Project = apiBuild,
                                    Input = sourceOutput,
                                    Outputs = new[] { apiBuildOutput },
                                    RunOrder = 2,
                                }),
                                new CodeBuildAction(new CodeBuildActionProps
                                {
                                    ActionName = "CdkBuild",
                                    Project = cdkBuild,
                                    Input = sourceOutput,
                                    Outputs = new[] { cdkBuildOutput },
                                    RunOrder = 3,
                                }),
                            },
                        },
                        new StageProps
                        {
                            StageName = "Deploy",
                            Actions = new IAction[]
                            {
                                new CloudFormationCreateUpdateStackAction(new CloudFormationCreateUpdateStackActionProps
                                {
                                    ActionName = "ApiStack",
                                    TemplatePath = cdkBuildOutput.AtPath($"{props.ApiStackName}.template.json"),
                                    StackName = "Api",
                                    AdminPermissions = true,
                                    ParameterOverrides = new Dictionary<string, object>
                                    {
                                        [props.ApiImageTag] = apiBuildOutput.GetParam("metadata.json", "imageTag"),
                                    },
                                    ExtraInputs = new[]
                                    {
                                        apiBuildOutput,
                                    },
                                }),
                            },
                        },
                    },
                });
        }
    }
}
