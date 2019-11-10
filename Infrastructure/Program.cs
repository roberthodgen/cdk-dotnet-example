namespace Infrastructure
{
    using Amazon.CDK;

    internal static class Program
    {
        private static void Main()
        {
            var app = new App(new AppProps());
            var networkStack = new NetworkStack(app, "Network", new StackProps());
            var pipelineStack = new PipelineStack(app, "Pipeline", new StackProps());
            new ApiStack(app, "Api", new StackProps(), networkStack.Vpc, pipelineStack.EcrRepository);
            app.Synth();
        }
    }
}
