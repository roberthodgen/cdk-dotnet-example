namespace Infrastructure.Stacks
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.ECR;

    public class ApiStackProps : StackProps, IApiStackProps
    {
        public IVpc Vpc { get; set; }

        public IRepository Repository { get; set; }
    }
}
