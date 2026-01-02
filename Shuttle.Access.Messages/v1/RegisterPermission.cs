namespace Shuttle.Access.Messages.v1;

public class RegisterPermission
{
    public string Description { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
}