namespace Course.E2ETest.Infrastructure;

/// <summary>
/// Agrupa las pruebas del flujo Home para compartir un unico WebAppFixture
/// (evita levantar/derribar Course.Api y Course.Web en cada test).
/// </summary>
[CollectionDefinition("HomeE2E")]
public class HomeE2ECollection : ICollectionFixture<WebAppFixture>
{
}
