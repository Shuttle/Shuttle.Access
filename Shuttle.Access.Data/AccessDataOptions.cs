namespace Shuttle.Access.Data;

public class AccessDataOptions
{
    public const string SectionName = "Shuttle:Access:Data";

    public string ConnectionString { get; set; } = string.Empty;
}