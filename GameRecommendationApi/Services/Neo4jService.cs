using Neo4j.Driver; 

namespace GameRecommendationApi.Services;

public sealed class Neo4jService : IDisposable
{
    private readonly IDriver _driver; // "pool" konekcija
    public Neo4jService(IConfiguration configuration) // podaci iz appsettings.json fajla
    {
        var uri = configuration["Neo4j:Uri"] ?? "bolt://localhost:7687";
        var username = configuration["Neo4j:Username"] ?? "neo4j";
        var password = configuration["Neo4j:Password"] ?? "password123";

        // povezivanje sa bazom       
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
    }

    public IDriver Driver => _driver;

    public void Dispose()
    {
        // Kada se tvoja aplikacija gasi, ova metoda zatvara sve otvorene konekcije u "pool-u"
        _driver?.Dispose();
    }
}