namespace Auth.Config;

  public class ApiConfig
  {
  public static string SectionName = "ApiConfig";
  public int Port { get; init; }
  public string Service { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public TimeOnly Time { get; init; } = TimeOnly.FromDateTime(DateTime.UtcNow);
  }