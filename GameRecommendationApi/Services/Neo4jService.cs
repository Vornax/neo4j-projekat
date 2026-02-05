using Neo4j.Driver; 

namespace GameRecommendationApi.Services;

public sealed class Neo4jService : IDisposable
{
    private readonly IDriver _driver; // konekcioni pool

    // Jednostavan konstruktor: pročita osnovne vrednosti iz konfiguracije i kreira driver.
    public Neo4jService(IConfiguration configuration)
    {
        // čitljivo i lako za objasniti studentu/profesoru
        var uri = configuration["Neo4j:Uri"] ?? Environment.GetEnvironmentVariable("NEO4J_URI") ?? "bolt://localhost:7687";
        var username = configuration["Neo4j:Username"] ?? Environment.GetEnvironmentVariable("NEO4J_USERNAME") ?? "neo4j";
        var password = configuration["Neo4j:Password"] ?? Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? "password123";

        password = password?.Trim('"');

        // osnovna validacija URI-ja (da ne pokušavamo sa nečim očigledno pogrešnim)
        if (!Uri.TryCreate(uri, UriKind.Absolute, out _))
            throw new InvalidOperationException($"Neo4j URI is invalid: '{uri}'");

        try
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
            // kratka provera veze da bi se greške videly odmah pri startu
            _driver.VerifyConnectivityAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            // jednostavna i jasna poruka za debug
            Console.Error.WriteLine($"Neo4j connect failed: {ex.Message}");
            throw;
        }
    }

    public IDriver Driver => _driver;

    public void Dispose()
    {
        // zatvori konekcioni pool
        _driver?.Dispose();
    }
}