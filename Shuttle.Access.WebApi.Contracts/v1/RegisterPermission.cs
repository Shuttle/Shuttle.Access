namespace Shuttle.Access.WebApi.Contracts.v1;

public class RegisterPermission
{
    public Guid? Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
}