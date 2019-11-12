namespace Infrastructure
{
    using Amazon.CDK;
    using Stacks;
    using Stacks.Props;

    internal static class Program
    {
        private static readonly string apiImageTag = "ImageTag";
        private static readonly string apiStackName = "ExampleApi";
        private static readonly string gitHubOwner = "roberthodgen";
        private static readonly string gitHubRepo = "cdk-dotnet-example";

        private static void Main()
        {
            var app = new App(new AppProps());
            var networkStack = new NetworkStack(app, "ExampleNetwork", new StackProps());

            var pipelineStack = new PipelineStack(app, "ExamplePipeline", new PipelineStackProps
            {
                GitHubSecretName = "github.com/roberthodgen",
                ApiImageTag = apiImageTag,
                ApiStackName = apiStackName,
                GitHubOwner = gitHubOwner,
                GitHubRepo = gitHubRepo,
            });

            new IntegrationTestStack(app, "ExampleIntegrationTests", new IntegrationTestStackProps
            {
                GitHubOwner = gitHubOwner,
                GitHubRepo = gitHubRepo,
            });

            new ApiStack(app, apiStackName, new ApiStackProps
            {
                Vpc = networkStack.Vpc,
                Repository = pipelineStack.EcrRepository,
                ApiImageTag = apiImageTag,
            });

            app.Synth();
        }
    }
}
