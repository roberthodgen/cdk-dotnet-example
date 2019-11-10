namespace Infrastructure.Stacks
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.ECR;
    using Amazon.CDK.AWS.ECS;
    using Amazon.CDK.AWS.ECS.Patterns;
    using Amazon.CDK.AWS.ElasticLoadBalancingV2;

    public class ApiStack : Stack
    {
        public ApiStack(Construct parent, string id, IApiStackProps props) : base(parent, id, props)
        {
            var cluster = new Cluster(
                this,
                "Example",
                new ClusterProps
                {
                    Vpc = props.Vpc,
                });

            var logging = new AwsLogDriver(new AwsLogDriverProps
            {
                StreamPrefix = "Example",
            });

            var taskDef = new FargateTaskDefinition(
                this,
                "Task",
                new FargateTaskDefinitionProps
                {
                    MemoryLimitMiB = 512,
                    Cpu = 256,
                });

            var imageTag = new CfnParameter(
                this,
                "ImageTag",
                new CfnParameterProps
                {
                    Default = "latest",
                });

            var repo = Repository.FromRepositoryName(
                this,
                "EcrRepository",
                props.Repository.RepositoryName);

            var container = new ContainerDefinition(
                this,
                "ApiContainer",
                new ContainerDefinitionProps
                {
                    TaskDefinition = taskDef,
                    Image = ContainerImage.FromEcrRepository(repo, imageTag.ValueAsString),
                    Logging = logging,
                });

            container.AddPortMappings(new PortMapping
            {
                ContainerPort = 80,
                HostPort = 80,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP,
            });

            var loadBalancer = new ApplicationLoadBalancer(
                this,
                "LoadBalancer",
                new ApplicationLoadBalancerProps
                {
                    Vpc = props.Vpc,
                    Http2Enabled = false,
                    IdleTimeout = Duration.Seconds(5),
                    InternetFacing = true,
                    IpAddressType = IpAddressType.IPV4,
                    VpcSubnets = new SubnetSelection
                    {
                        Subnets = props.Vpc.PublicSubnets,
                    },
                });

            var ecsService = new ApplicationLoadBalancedFargateService(
                this,
                "Service",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = cluster,
                    TaskDefinition = taskDef,
                    AssignPublicIp = false,
                    PublicLoadBalancer = true,
                    LoadBalancer = loadBalancer,
                });

            PrintLoadBalancerDnsName(ecsService);
        }

        private CfnOutput PrintLoadBalancerDnsName(ApplicationLoadBalancedServiceBase ecsService)
        {
            return new CfnOutput(
                this,
                "LoadBalancerDnsName",
                new CfnOutputProps
                {
                    Value = ecsService.LoadBalancer.LoadBalancerDnsName
                });
        }
    }
}
