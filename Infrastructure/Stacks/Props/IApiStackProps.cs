namespace Infrastructure.Stacks.Props
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.ECR;

    public interface IApiStackProps : IStackProps
    {
        IVpc Vpc { get; }

        IRepository Repository { get; }

        string ApiImageTag { get; }
    }
}
