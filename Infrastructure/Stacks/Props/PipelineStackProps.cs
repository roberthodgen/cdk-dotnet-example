namespace Infrastructure.Stacks.Props
{
    using Amazon.CDK;

    public class PipelineStackProps : StackProps, IPipelineStackProps
    {
        public string GitHubSecretName { get; set; }

        public string ApiImageTag { get; set; }

        public string ApiStackName { get; set; }

        public string GitHubRepo { get; set; }

        public string GitHubOwner { get; set; }
    }
}
