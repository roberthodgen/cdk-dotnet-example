namespace Infrastructure.Stacks.Props
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.ECR;

    public class ApiStackProps : StackProps, IApiStackProps
    {
        public IVpc Vpc { get; set; }

        public IRepository Repository { get; set; }

        public string ApiImageTag { get; set; }
    }
}
