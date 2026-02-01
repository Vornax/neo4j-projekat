using GameRecommendationApi.Services.Interfaces;
using Neo4j.Driver;

namespace GameRecommendationApi.Services;

public class MetadataService : IMetadataService
{
    private readonly Neo4jService _neo4j;
    public MetadataService(Neo4jService neo4j) => _neo4j = neo4j;

    public async Task<List<string>> GetAllGenresAsync()
    {
        await using var session = _neo4j.Driver.AsyncSession(); // AsyncSession(): Kreira se veza(sesija) sa bazom. Sesije služe za izvršavanje upita
        var cypher = @"MATCH (g:Genre) RETURN g.name AS name ORDER BY name";
        var cursor = await session.RunAsync(cypher); //RunAsync: Šalje upit bazi podataka
        var records = await cursor.ToListAsync(); // povlači podatke sa servera i smešta u obliku liste
        return records.Select(r => r["name"].As<string>()).ToList(); // transformise podatke iz formata baze u format List<string>

    }

    public async Task<List<string>> GetAllMechanicsAsync()
    {
        await using var session = _neo4j.Driver.AsyncSession();
        var cypher = @"MATCH (m:Mechanic) RETURN m.name AS name ORDER BY name";
        var cursor = await session.RunAsync(cypher);
        var records = await cursor.ToListAsync();
        return records.Select(r => r["name"].As<string>()).ToList();
    }

    public async Task<List<string>> GetAllDevelopersAsync()
    {
        await using var session = _neo4j.Driver.AsyncSession();
        var cypher = @"MATCH (d:Developer) RETURN d.name AS name ORDER BY name";
        var cursor = await session.RunAsync(cypher);
        var records = await cursor.ToListAsync();
        return records.Select(r => r["name"].As<string>()).ToList();
    }
}