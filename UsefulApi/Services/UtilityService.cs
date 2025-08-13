
// Business logic implementation
public class UtilityService : IUtilityService
{
    public object GetHealth() => new { status = "ok", service = "useful-api", version = "1.0.0", timeUtc = DateTimeOffset.UtcNow };
    public object GetTime() => new { utc = DateTimeOffset.UtcNow, unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), iso = DateTimeOffset.UtcNow.ToString("o") };
    public object GenerateGuid() => new { value = Guid.NewGuid(), kind = "random-v4" };
}