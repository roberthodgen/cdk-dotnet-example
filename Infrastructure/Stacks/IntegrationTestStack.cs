namespace Infrastructure.Stacks
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.CodeBuild;
    using Props;

    public class IntegrationTestStack : Stack
    {
        public IntegrationTestStack(Construct parent, string id, IIntegrationTestStackProps props)
            : base(parent, id, props)
        {
            new Project(
                this,
                "ExampleIntegrationTests",
                new ProjectProps
                {
                    BuildSpec = BuildSpec.FromSourceFilename("Infrastructure/Resources/ci_buildspec.yml"),
                    Source = Source.GitHub(new GitHubSourceProps
                    {
                        Owner = props.GitHubOwner,
                        Repo = props.GitHubRepo,
                        ReportBuildStatus = true,
                        CloneDepth = 1,
                        WebhookFilters = new[]
                        {
                            FilterGroup.InEventOf(
                                EventAction.PULL_REQUEST_CREATED,
                                EventAction.PULL_REQUEST_UPDATED,
                                EventAction.PULL_REQUEST_REOPENED),
                        },
                    }),
                    Environment = new BuildEnvironment
                    {
                        BuildImage = LinuxBuildImage.AMAZON_LINUX_2,
                        Privileged = true,
                    },
                });
        }
    }
}
