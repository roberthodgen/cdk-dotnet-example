namespace Infrastructure
{
    using Amazon.CDK;
    using Stacks;
    using Stacks.Props;

    internal static class Program
    {
        private static readonly string apiImageTag = "ImageTag";

        private static void Main()
        {
            var app = new App(new AppProps());
            var networkStack = new NetworkStack(app, "Network", new StackProps());

            var pipelineStack = new PipelineStack(app, "Pipeline", new PipelineStackProps
            {
                GitHubSecretName = "github.com/roberthodgen",
                ApiImageTag = apiImageTag,
            });

            new IntegrationTestStack(app, "IntegrationTest", new StackProps());

            new ApiStack(app, "Api", new ApiStackProps
            {
                Vpc = networkStack.Vpc,
                Repository = pipelineStack.EcrRepository,
                ApiImageTag = apiImageTag,
            });

            app.Synth();
        }
    }
}
