namespace Infrastructure.Stacks
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.CodeBuild;

    public class IntegrationTestStack : Stack
    {
        public IntegrationTestStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            new Project(
                this,
                "ApiIntegrationTest",
                new ProjectProps
                {
                    BuildSpec = BuildSpec.FromSourceFilename("Infrastructure/Resources/ci_buildspec.yml"),
                    Source = Source.GitHub(new GitHubSourceProps
                    {
                        Owner = "roberthodgen",
                        Repo = "cdk-dotnet-example",
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
                });
        }
    }
}
