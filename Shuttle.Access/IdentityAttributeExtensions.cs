using System.Globalization;
using System.Linq;
using Shuttle.Contract;

namespace Shuttle.Access;

public static class IdentityAttributeExtensions
{
    extension(Identity identity)
    {
        public bool? GetAttributeAsBoolean(Query.AttributeDefinition attributeDefinition)
        {
            var value = identity.GetSingleAttributeValue(attributeDefinition, AttributeType.Boolean);

            return value == null ? null : bool.Parse(value);
        }

        public DateTimeOffset? GetAttributeAsDateTimeOffset(Query.AttributeDefinition attributeDefinition)
        {
            var value = identity.GetSingleAttributeValue(attributeDefinition, AttributeType.DateTimeOffset);

            return value == null ? null : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
        }

        public decimal? GetAttributeAsDecimal(Query.AttributeDefinition attributeDefinition)
        {
            var value = identity.GetSingleAttributeValue(attributeDefinition, AttributeType.Decimal);

            return value == null ? null : decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        public Guid? GetAttributeAsGuid(Query.AttributeDefinition attributeDefinition)
        {
            var value = identity.GetSingleAttributeValue(attributeDefinition, AttributeType.Guid);

            return value == null ? null : Guid.Parse(value);
        }

        public int? GetAttributeAsInteger(Query.AttributeDefinition attributeDefinition)
        {
            var value = identity.GetSingleAttributeValue(attributeDefinition, AttributeType.Integer);

            return value == null ? null : int.Parse(value, CultureInfo.InvariantCulture);
        }

        public string? GetAttributeAsString(Query.AttributeDefinition attributeDefinition)
        {
            return identity.GetSingleAttributeValue(attributeDefinition, AttributeType.String);
        }

        public IEnumerable<bool> GetAttributeValuesAsBoolean(Query.AttributeDefinition attributeDefinition)
        {
            return identity.GetTypedAttributeValues(attributeDefinition, AttributeType.Boolean).Select(bool.Parse);
        }

        public IEnumerable<DateTimeOffset> GetAttributeValuesAsDateTimeOffset(Query.AttributeDefinition attributeDefinition)
        {
            return identity.GetTypedAttributeValues(attributeDefinition, AttributeType.DateTimeOffset)
                .Select(value => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture));
        }

        public IEnumerable<decimal> GetAttributeValuesAsDecimal(Query.AttributeDefinition attributeDefinition)
        {
            return identity.GetTypedAttributeValues(attributeDefinition, AttributeType.Decimal)
                .Select(value => decimal.Parse(value, CultureInfo.InvariantCulture));
        }

        public IEnumerable<Guid> GetAttributeValuesAsGuid(Query.AttributeDefinition attributeDefinition)
        {
            return identity.GetTypedAttributeValues(attributeDefinition, AttributeType.Guid).Select(Guid.Parse);
        }

        public IEnumerable<int> GetAttributeValuesAsInteger(Query.AttributeDefinition attributeDefinition)
        {
            return identity.GetTypedAttributeValues(attributeDefinition, AttributeType.Integer)
                .Select(value => int.Parse(value, CultureInfo.InvariantCulture));
        }

        public IEnumerable<string> GetAttributeValuesAsString(Query.AttributeDefinition attributeDefinition)
        {
            return identity.GetTypedAttributeValues(attributeDefinition, AttributeType.String);
        }

        private string? GetSingleAttributeValue(Query.AttributeDefinition attributeDefinition, AttributeType expectedType)
        {
            Guard.AgainstNull(attributeDefinition);

            if (attributeDefinition.Cardinality != AttributeCardinality.Single)
            {
                throw new InvalidOperationException(string.Format(Resources.AttributeCardinalityMismatchException,
                    attributeDefinition.Name, attributeDefinition.Cardinality));
            }

            return identity.GetTypedAttributeValues(attributeDefinition, expectedType).SingleOrDefault();
        }

        private IEnumerable<string> GetTypedAttributeValues(Query.AttributeDefinition attributeDefinition, AttributeType expectedType)
        {
            Guard.AgainstNull(identity);
            Guard.AgainstNull(attributeDefinition);

            if (attributeDefinition.Type != expectedType)
            {
                throw new InvalidOperationException(string.Format(Resources.AttributeTypeMismatchException,
                    attributeDefinition.Name, attributeDefinition.Type, expectedType));
            }

            return identity.GetAttributeValues(attributeDefinition.Id);
        }
    }
}
