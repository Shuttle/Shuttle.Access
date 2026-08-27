using System.Globalization;
using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityAttributeValueParticipant(IEventStore eventStore, IAttributeDefinitionQuery attributeDefinitionQuery) : IParticipant<SetIdentityAttributeValue>
{
    private readonly IAttributeDefinitionQuery _attributeDefinitionQuery = Guard.AgainstNull(attributeDefinitionQuery);
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetIdentityAttributeValue message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var attributeDefinition = await _attributeDefinitionQuery.FindAsync(
                new Query.AttributeDefinition.Specification().AddId(message.AttributeDefinitionId), cancellationToken)
            ?? throw new ApplicationException($"Could not find an attribute definition with id '{message.AttributeDefinitionId}'.");

        if (message.Active && !IsValidValue(attributeDefinition.Type, message.Value))
        {
            throw new DomainException(string.Format(Shuttle.Access.Resources.AttributeValueFormatException, message.Value,
                attributeDefinition.Name, attributeDefinition.Type));
        }

        var stream = await _eventStore.GetAsync(message.IdentityId, cancellationToken);
        var identity = stream.Get<Identity>();

        if (message.Active)
        {
            if (identity.HasAttributeValue(attributeDefinition.Id, message.Value))
            {
                return;
            }

            if (attributeDefinition.Cardinality == AttributeCardinality.Single)
            {
                foreach (var existingValue in identity.GetAttributeValues(attributeDefinition.Id).ToList())
                {
                    stream.Add(identity.RemoveAttributeValue(attributeDefinition.Id, existingValue));
                }
            }

            stream.Add(identity.AddAttributeValue(attributeDefinition.Id, message.Value));
        }
        else
        {
            if (!identity.HasAttributeValue(attributeDefinition.Id, message.Value))
            {
                return;
            }

            stream.Add(identity.RemoveAttributeValue(attributeDefinition.Id, message.Value));
        }

        if (stream.ShouldSave())
        {
            await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }
    }

    private static bool IsValidValue(AttributeType type, string value)
    {
        return type switch
        {
            AttributeType.String => true,
            AttributeType.Integer => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            AttributeType.Decimal => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _),
            AttributeType.Boolean => bool.TryParse(value, out _),
            AttributeType.DateTimeOffset => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
            AttributeType.Guid => Guid.TryParse(value, out _),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
