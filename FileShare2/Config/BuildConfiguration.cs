public class BuildDockerConfiguration
{
    public string AWS_REGION { get; set; }
    public string AWS_ACCOUNT_ID { get; set; }
    public string IMAGE_TAG { get; set; } = "latest";
}

public class BuildConfiguration
{
    public BuildDockerConfiguration build_docker {  get; set; }
}