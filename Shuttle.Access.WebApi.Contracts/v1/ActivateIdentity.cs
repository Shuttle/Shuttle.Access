namespace Shuttle.Access.WebApi.Contracts.v1;

public class ActivateIdentity
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}