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
                SubnetType = SubnetType.PUBLIC,
                Name = "Ingress",
                CidrMask = 24,
            };

            var applicationSubnet = new SubnetConfiguration
            {
                SubnetType = SubnetType.PRIVATE,
                Name = "Application",
                CidrMask = 24,
            };

            var databaseSubnet = new SubnetConfiguration
            {
                SubnetType = SubnetType.ISOLATED,
                Name = "Database",
                CidrMask = 24,
            };

            Vpc = new Vpc(
                this,
                "Dev",
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
                    Value = "Public Subnet IDs: " + string.Join(", ", Vpc.PublicSubnets.Select(s => s.SubnetId)),
                });
        }

        private void PrintPrivateSubnetIds()
        {
            new CfnOutput(
                this,
                "PrivateSubnetIds",
                new CfnOutputProps
                {
                    Value = "Private Subnet IDs: " + string.Join(", ", Vpc.PrivateSubnets.Select(s => s.SubnetId)),
                });
        }

        private void PrintIsolatedSubnetIds()
        {
            new CfnOutput(
                this,
                "IsolatedSubnetIds",
                new CfnOutputProps
                {
                    Value = "Isolated Subnet IDs: " + string.Join(" ,", Vpc.IsolatedSubnets.Select(s => s.SubnetId)),
                });
        }
    }
}
