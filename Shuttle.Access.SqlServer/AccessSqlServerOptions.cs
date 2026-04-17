namespace Shuttle.Access.SqlServer;

public class AccessSqlServerOptions
{
    public const string SectionName = "Shuttle:Access:SqlServer";

    public string ConnectionString { get; set; } = string.Empty;
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
}