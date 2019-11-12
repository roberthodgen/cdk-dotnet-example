namespace Infrastructure.Stacks.Props
{
    using Amazon.CDK;

    public interface IIntegrationTestStackProps : IStackProps
    {
        string GitHubRepo { get; }

        string GitHubOwner { get; }
    }
}
