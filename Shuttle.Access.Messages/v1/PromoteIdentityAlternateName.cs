namespace Shuttle.Access.Messages.v1;

public class PromoteIdentityAlternateName : AuditMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 1 = Replace (the alternate becomes the primary name; the previous primary name is discarded),
    /// 2 = Swap (the alternate becomes the primary name; the previous primary name becomes an alternate).
    /// </summary>
    public int Mode { get; set; }
}
