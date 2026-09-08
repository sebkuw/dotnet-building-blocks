using sebkuw.Domain.Abstractions.Persistence;

namespace sebkuw.Domain.Abstractions.History;

/// <summary>
/// Marks an entity whose persisted records are immutable and may only be appended.
/// </summary>
public interface IAppendOnlyEntity : IHardDeleteProtected;
