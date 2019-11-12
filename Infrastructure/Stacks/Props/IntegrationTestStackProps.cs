namespace Infrastructure.Stacks.Props
{
    using Amazon.CDK;

    public class IntegrationTestStackProps : StackProps, IIntegrationTestStackProps
    {
        public string GitHubRepo { get; set; }

        public string GitHubOwner { get; set; }
    }
}
