namespace Auth.Config;

public class AppConfig
{
  public static string SectionName = "AppConfig";
  public int Port { get; init; }
  public string Service { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public string ConnectionString { get; init; } = string.Empty;
  public TimeOnly Time { get; init; } = TimeOnly.FromDateTime(DateTime.UtcNow);
  }