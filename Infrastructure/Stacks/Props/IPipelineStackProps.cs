namespace Infrastructure.Stacks.Props
{
    using Amazon.CDK;

    public interface IPipelineStackProps : IStackProps
    {
        string GitHubSecretName { get; }

        string ApiImageTag { get; }

        string ApiStackName { get; }

        string GitHubRepo { get; }

        string GitHubOwner { get; }
    }
}
