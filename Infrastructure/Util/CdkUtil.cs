namespace Infrastructure.Util
{
    using Amazon.CDK.AWS.CodeBuild;

    public static class CdkUtil
    {
        public static IBuildEnvironmentVariable PlainTextBuildEnvironmentVariable(string value)
        {
            return new BuildEnvironmentVariable
            {
                Type = BuildEnvironmentVariableType.PLAINTEXT,
                Value = value,
            };
        }
    }
}
