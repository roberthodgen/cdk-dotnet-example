namespace Infrastructure
{
    using System.Linq;
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;

    public class NetworkStack : Stack
    {
        public IVpc Vpc { get; }

        public NetworkStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            var ingressSubnet = new SubnetConfiguration
            {
                Name = "Ingress",
                SubnetType = SubnetType.PUBLIC,
                CidrMask = 24,
            };

            var applicationSubnet = new SubnetConfiguration
            {
                Name = "Application",
                SubnetType = SubnetType.PRIVATE,
                CidrMask = 24,
            };

            var databaseSubnet = new SubnetConfiguration
            {
                Name = "Database",
                SubnetType = SubnetType.ISOLATED,
                CidrMask = 24,
            };

            Vpc = new Vpc(
                this,
                "Primary",
                new VpcProps
                {
                    Cidr = "10.1.0.0/16",
                    MaxAzs = 2,
                    NatGateways = 1,
                    SubnetConfiguration = new ISubnetConfiguration[]
                    {
                        ingressSubnet,
                        applicationSubnet,
                        databaseSubnet,
                    },
                });

            PrintPublicSubnetIds();
            PrintPrivateSubnetIds();
            PrintIsolatedSubnetIds();
        }

        private void PrintPublicSubnetIds()
        {
            new CfnOutput(
                this,
                "PublicSubnetIds",
                new CfnOutputProps
                {
                    ExportName = "PublicSubnetIds",
                    Value = "ids:" + string.Join(",", Vpc.PublicSubnets.Select(s => s.SubnetId)),
                });
        }

        private void PrintPrivateSubnetIds()
        {
            new CfnOutput(
                this,
                "PrivateSubnetIds",
                new CfnOutputProps
                {
                    ExportName = "PrivateSubnetIds",
                    Value = "ids:" + string.Join(",", Vpc.PrivateSubnets.Select(s => s.SubnetId)),
                });
        }

        private void PrintIsolatedSubnetIds()
        {
            new CfnOutput(
                this,
                "IsolatedSubnetIds",
                new CfnOutputProps
                {
                    ExportName = "IsolatedSubnetIds",
                    Value = "ids:" + string.Join(",", Vpc.IsolatedSubnets.Select(s => s.SubnetId)),
                });
        }
    }
}
