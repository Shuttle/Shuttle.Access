using Shuttle.Access.Events.AttributeDefinition.v1;
using Shuttle.Contract;

namespace Shuttle.Access;

public enum AttributeType
{
    String = 1,
    Integer = 2,
    Decimal = 3,
    Boolean = 4,
    DateTimeOffset = 5,
    Guid = 6
}

public enum AttributeCardinality
{
    Single = 1,
    Multiple = 2
}

public class AttributeDefinition
{
    public AttributeCardinality Cardinality { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool Removed { get; private set; }
    public Guid TenantId { get; private set; }
    public AttributeType Type { get; private set; }

    public static string Key(string name, Guid tenantId)
    {
        return $"[attribute-definition]:name={Guard.AgainstEmpty(name)};tenant-id={Guard.AgainstEmpty(tenantId):D}";
    }

    private Registered On(Registered registered)
    {
        Guard.AgainstNull(registered);

        TenantId = registered.TenantId;
        Name = registered.Name;
        Description = registered.Description;
        Type = registered.Type;
        Cardinality = registered.Cardinality;

        Removed = false;

        return registered;
    }

    private NameSet On(NameSet nameSet)
    {
        Guard.AgainstNull(nameSet);

        Name = nameSet.Name;

        return nameSet;
    }

    private DescriptionSet On(DescriptionSet descriptionSet)
    {
        Guard.AgainstNull(descriptionSet);

        Description = descriptionSet.Description;

        return descriptionSet;
    }

    private Removed On(Removed removed)
    {
        Guard.AgainstNull(removed);

        Removed = true;

        return removed;
    }

    public Registered Register(Guid tenantId, string name, string description, AttributeType type, AttributeCardinality cardinality)
    {
        return On(new Registered
        {
            TenantId = tenantId,
            Name = Guard.AgainstEmpty(name),
            Description = description,
            Type = type,
            Cardinality = cardinality
        });
    }

    public Removed Remove()
    {
        return On(new Removed());
    }

    public DescriptionSet SetDescription(string description)
    {
        if (description.Equals(Description))
        {
            throw new DomainException(string.Format(Resources.PropertyUnchangedException, "Description", Description));
        }

        return On(new DescriptionSet
        {
            Description = description
        });
    }

    public NameSet SetName(string name)
    {
        if (name.Equals(Name))
        {
            throw new DomainException(string.Format(Resources.PropertyUnchangedException, "Name", Name));
        }

        return On(new NameSet
        {
            Name = name
        });
    }
}
