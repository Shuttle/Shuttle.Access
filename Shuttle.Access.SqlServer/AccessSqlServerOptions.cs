namespace Shuttle.Access.SqlServer;

public class AccessSqlServerOptions
{
    public const string SectionName = "Shuttle:Access:Data";

    public string ConnectionString { get; set; } = string.Empty;
}