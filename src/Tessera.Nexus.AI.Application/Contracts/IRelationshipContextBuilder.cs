using Tessera.Nexus.AI.Domain.Entities;

namespace Tessera.Nexus.AI.Application.Contracts;

public interface IRelationshipContextBuilder
{
    string BuildRelationshipContext(
        IReadOnlyList<EpicorRelation> relationships);
}