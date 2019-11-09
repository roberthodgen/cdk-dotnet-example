namespace Infrastructure
{
    using System.Collections.Generic;
    using Amazon.CDK;
    using Amazon.CDK.AWS.CodeBuild;
    using Amazon.CDK.AWS.CodePipeline;
    using Amazon.CDK.AWS.CodePipeline.Actions;

    public class PipelineStack : Stack
    {
        public PipelineStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            var cdkBuild = new PipelineProject(
                this,
                "CdkBuild",
                new PipelineProjectProps
                {
                    BuildSpec = BuildSpec.FromSourceFilename("Resources/cdk_buildspec.yml"),
                    Environment = new BuildEnvironment
                    {
                        BuildImage = LinuxBuildImage.AMAZON_LINUX_2,
                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                        {
                            ["AWS_ACCOUNT_ID"] = BuildEnvironmentVariable(Of(this).Account),
                            ["AWS_DEFAULT_REGION"] = BuildEnvironmentVariable(Of(this).Region),
                        },
                    },
                });

            var apiBuild = new PipelineProject(
                this,
                "ApiBuild",
                new PipelineProjectProps
                {
                    BuildSpec = BuildSpec.FromSourceFilename("Resources/api_buildspec.yml"),
                    Environment = new BuildEnvironment
                    {
                        BuildImage = LinuxBuildImage.AMAZON_LINUX_2,
                    },
                });

            var apiTest = new PipelineProject(
                this,
                "ApiTest",
                new PipelineProjectProps
                {
                    BuildSpec = BuildSpec.FromSourceFilename("Resources/ci_buildspec.yml"),
                    Environment = new BuildEnvironment
                    {
                        BuildImage = LinuxBuildImage.AMAZON_LINUX_2,
                    },
                });

            var sourceOutput = new Artifact_();
            var cdkBuildOutput = new Artifact_("CdkBuildOutput");
            var apiBuildOutput = new Artifact_("ApiBuildOutput");

            new Pipeline(
                this,
                "Pipeline",
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
                                    ActionName = "GitHubSource",
                                    OauthToken = SecretValue.SecretsManager("github.com/roberthodgen"),
                                    Repo = "cdk-dotnet-example",
                                    Owner = "roberthodgen",
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
                                    ActionName = "CloudFormation",
                                    TemplatePath = cdkBuildOutput.AtPath("ApiStack.template.json"),
                                    StackName = "ApiStack",
                                    AdminPermissions = true,
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

        private static IBuildEnvironmentVariable BuildEnvironmentVariable(string value)
        {
            return new BuildEnvironmentVariable
            {
                Type = BuildEnvironmentVariableType.PLAINTEXT,
                Value = value,
            };
        }
    }
}
