using Amazon.CDK;

namespace Infrastructure
{
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.ECS;
    using Amazon.CDK.AWS.ECS.Patterns;
    using Amazon.CDK.AWS.IAM;
    using Protocol = Amazon.CDK.AWS.ECS.Protocol;

    public class CdkHelloWorldStack : Stack
    {
        public CdkHelloWorldStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            var ingressSubnet = new SubnetConfiguration
            {
                SubnetType = SubnetType.PUBLIC,
                Name = "Ingress",
            };

            var applicationSubnet = new SubnetConfiguration
            {
                SubnetType = SubnetType.PRIVATE,
                Name = "Application",
            };
            
            var databaseSubnet = new SubnetConfiguration
            {
                SubnetType = SubnetType.ISOLATED,
                Name = "Database",
            };

            var vpc = new Vpc(
                this,
                "Dev",
                new VpcProps
                {
                    MaxAzs = 2,
                    NatGateways = 1,
                    SubnetConfiguration = new []
                    {
                        ingressSubnet,
                        applicationSubnet,
                        databaseSubnet,
                    } 
                });

            var cluster = new Cluster(
                this,
                "Proxy",
                new ClusterProps
                {
                    Vpc = vpc,
                });
            
            var host = new BastionHostLinux(this, "BastionHost", new BastionHostLinuxProps
            {
                Vpc = vpc,
                SubnetSelection = new SubnetSelection
                {
                    SubnetType = SubnetType.PUBLIC,
                },
                InstanceType = new InstanceType("t2.micro"),
            });

            var securityGroup = new SecurityGroup(this, "HttpReverseProxy", new SecurityGroupProps
            {
                Vpc = vpc,
            });

            securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(80), "IPv4 HTTP Ingress");
            securityGroup.AddIngressRule(Peer.AnyIpv6(), Port.Tcp(80), "IPv6 HTTP Ingress");

            securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(443), "IPv4 HTTPS Ingress");
            securityGroup.AddIngressRule(Peer.AnyIpv6(), Port.Tcp(443), "IPv6 HTTPS Ingress");

            securityGroup.AddEgressRule(Peer.AnyIpv4(), Port.Tcp(80));

            // create a task definition with CloudWatch Logs
            var logging = new AwsLogDriver(new AwsLogDriverProps
            {
                StreamPrefix = "nginx",
            });

            var taskRole = new Role(
                this,
                "ProxyRole",
                new RoleProps
                {
                    ManagedPolicies = new[] { ManagedPolicy.FromAwsManagedPolicyName("") },
                    AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
                });

            var taskDef = new FargateTaskDefinition(
                this,
                "MyTaskDefinition",
                new FargateTaskDefinitionProps
                {
                    MemoryLimitMiB = 512,
                    Cpu = 256,
                    ExecutionRole = taskRole,
                });

            var image = ContainerImage.FromRegistry("nginx:latest");

            var container = new ContainerDefinition(this, "nginx", new ContainerDefinitionProps
            {
                TaskDefinition = taskDef,
                Image = image,
                Logging = logging,
            });

            container.AddPortMappings(new PortMapping
            {
                ContainerPort = 80,
                HostPort = 80,
                Protocol = Protocol.TCP,
            });

            container.AddPortMappings(new PortMapping
            {
                ContainerPort = 443,
                HostPort = 443,
                Protocol = Protocol.TCP,
            });


            var ecsService = new ApplicationLoadBalancedFargateService(
                this,
                "Ec2Service",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = cluster,
                    MemoryLimitMiB = 512,
                    TaskDefinition = taskDef,
                    AssignPublicIp = true,
                });

            var output = new CfnOutput(
                this,
                "LoadBalancerDNS",
                new CfnOutputProps
                {
                    Value = ecsService.LoadBalancer.LoadBalancerDnsName
                });
        }
    }
}
