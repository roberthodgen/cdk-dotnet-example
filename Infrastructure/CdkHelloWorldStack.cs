using Amazon.CDK;

namespace Infrastructure
{
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.ECS;
    using Amazon.CDK.AWS.ECS.Patterns;
    using Protocol = Amazon.CDK.AWS.ECS.Protocol;

    public class CdkHelloWorldStack : Stack
    {
        public CdkHelloWorldStack(Construct parent, string id, IStackProps props, IVpc vpc) : base(parent, id, props)
        {
            var cluster = new Cluster(
                this,
                "Proxy",
                new ClusterProps
                {
                    Vpc = vpc,
                });

            var securityGroup = new SecurityGroup(this, "HttpReverseProxy", new SecurityGroupProps
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

            var taskDef = new FargateTaskDefinition(
                this,
                "MyTaskDefinition",
                new FargateTaskDefinitionProps
                {
                    MemoryLimitMiB = 512,
                    Cpu = 256,
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
