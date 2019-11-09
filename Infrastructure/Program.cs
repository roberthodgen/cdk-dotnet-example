namespace Infrastructure
{
    using Amazon.CDK;

    internal static class Program
    {
        private static void Main()
        {
            var app = new App(new AppProps());
            var networking = new NetworkStack(app, "Network", new StackProps());
            new ApiStack(app, "Api", new StackProps(), networking.Vpc);
            new PipelineStack(app, "Pipeline", new StackProps());
            app.Synth();
        }
    }
}
