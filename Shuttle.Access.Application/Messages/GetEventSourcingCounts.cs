namespace Shuttle.Access.Application;

public class GetEventSourcingCounts
{
    public long MaximumSequenceNumber { get; set; }
    public bool HasUnsequencedPrimitiveEvents { get; set; }
    public bool HasWaitingProjections { get; set; }
}