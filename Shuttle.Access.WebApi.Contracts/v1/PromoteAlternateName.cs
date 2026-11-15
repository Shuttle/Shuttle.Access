namespace Shuttle.Access.WebApi.Contracts.v1;

public class PromoteAlternateName
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 1 = Replace (the alternate becomes the primary name; the previous primary name is discarded),
    /// 2 = Swap (the alternate becomes the primary name; the previous primary name becomes an alternate).
    /// </summary>
    public int Mode { get; set; }
}
