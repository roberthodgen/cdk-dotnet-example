namespace Infrastructure
{
    using Amazon.CDK;

    internal static class Program
    {
        private static void Main()
        {
            var app = new App(new AppProps());
            new CdkHelloWorldStack(app, "CdkHelloWorldStack", new StackProps());
            app.Synth();
        }
    }
}
