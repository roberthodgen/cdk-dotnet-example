namespace Infrastructure
{
    using Amazon.CDK;

    internal static class Program
    {
        private static void Main()
        {
            var app = new App(new AppProps());
            var networking = new NetworkStack(app, "Network", new StackProps());
            new CdkHelloWorldStack(app, "CdkHelloWorldStack", new StackProps(), networking.Vpc);
            app.Synth();
        }
    }
}
