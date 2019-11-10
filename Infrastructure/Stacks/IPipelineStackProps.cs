namespace Infrastructure.Stacks
{
    using Amazon.CDK;

    public interface IPipelineStackProps : IStackProps
    {
        string ApiImageTag { get; }
    }
}
