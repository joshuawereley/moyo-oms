using Xunit;

namespace Moyo.Oms.IntegrationTests;

/// <summary>
/// Binds the shared <see cref="SqlServerFixture"/> to every test class marked
/// with <c>[Collection("Database")]</c>, so the container starts only once.
/// </summary>

[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<SqlServerFixture>
{
}
