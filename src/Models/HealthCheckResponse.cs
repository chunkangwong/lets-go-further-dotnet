namespace controller_api_test.src.Models;

public class HealthCheckResponse
{
    public string Status { get; set; } = "available";
    public SystemInfoResponse SystemInfo { get; set; } = new SystemInfoResponse();
}

public class SystemInfoResponse
{
    public string Environment { get; set; } = "development";
    public string Version { get; set; } = "0.0.0";
}
