namespace Infrastructure.Stacks
{
    using Amazon.CDK;

    public class PipelineStackProps : StackProps, IPipelineStackProps
    {
        public string ApiImageTag { get; set; }
    }
}
