using GameRecommendationApi.Models;
using GameRecommendationApi.Services.Interfaces;
using Neo4j.Driver;

namespace GameRecommendationApi.Services;

public class UserService : IUserService
{
    private readonly Neo4jService _neo4j;
    public UserService(Neo4jService neo4j) => _neo4j = neo4j;

    public async Task<List<User>> GetAllUsersAsync()
    {
        await using var session = _neo4j.Driver.AsyncSession(); // AsyncSession(): Kreira se veza(sesija) sa bazom. Sesije služe za izvršavanje upita
        var cypher = @"MATCH (u:User) RETURN u.username AS username, u.role AS role ORDER BY username";
        var cursor = await session.RunAsync(cypher); //RunAsync: Šalje upit bazi podataka
        var records = await cursor.ToListAsync(); // povlači podatke sa servera i smešta u obliku liste
        return records.Select(r => new User { Username = r["username"].As<string>(), Role = r["role"].As<string>() }).ToList(); // transformise podatke iz formata baze u format List<User>
    }

    public async Task<List<int>> GetUserLikesAsync(string username)
    {
        await using var session = _neo4j.Driver.AsyncSession();
        var cypher = @"
            MATCH (u:User {username: $username})-[:LIKES]->(g:Game)
            RETURN g.id AS id
            ORDER BY g.title";
        var cursor = await session.RunAsync(cypher, new { username });
        var records = await cursor.ToListAsync();
        return records.Select(r => r["id"].As<int>()).ToList();
    }

    public async Task<bool> AddToWishlistAsync(string username, int gameId)
    {
        await using var session = _neo4j.Driver.AsyncSession();

        var cypher = @"
            MATCH (u:User {username: $username})
            MATCH (g:Game {id: $gameId})
            MERGE (u)-[:LIKES]->(g)";

        await session.RunAsync(cypher, new { username, gameId });
        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(string username, int gameId)
    {
        await using var session = _neo4j.Driver.AsyncSession();
        var cypher = @"
            MATCH (u:User {username: $username})-[r:LIKES]->(g:Game {id: $gameId})
            DELETE r";
        await session.RunAsync(cypher, new { username, gameId });
        return true;
    }
}