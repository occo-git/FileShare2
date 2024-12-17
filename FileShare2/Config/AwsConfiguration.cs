public class AwsConfiguration
{
    private const string CONS_DEFAULT_REGION = "eu-north-1";

    public string AccessKey { get; private set; }
    public string SecretKey { get; private set; }
    public Amazon.RegionEndpoint Region { get; private set; }

    public AwsConfiguration(string accessKey, string secretKey, string region = CONS_DEFAULT_REGION) 
    {
        AccessKey = accessKey;
        SecretKey = secretKey;
        Region = Amazon.RegionEndpoint.GetBySystemName(string.IsNullOrEmpty(region) ? CONS_DEFAULT_REGION  : region);
    }
}