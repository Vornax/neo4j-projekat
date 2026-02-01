using GameRecommendationApi.Models;
using GameRecommendationApi.Services.Interfaces;
using Neo4j.Driver;

namespace GameRecommendationApi.Services;

public class GameService : IGameService
{
    private readonly Neo4jService _neo4j;

    public GameService(Neo4jService neo4j)
    {
        _neo4j = neo4j;
    }

    public async Task<List<Game>> SearchGamesAsync(
        string? searchText,
        List<string>? genres,
        List<string>? developers,
        List<string>? mechanics,
        int? maxResults = 30)
    {
        await using var session = _neo4j.Driver.AsyncSession();

        var cypher = @"
            MATCH (g:Game)
            WHERE 
                ($searchText IS NULL OR $searchText = '' OR toLower(g.title) CONTAINS toLower($searchText))
                AND ($genres IS NULL OR size($genres) = 0 OR all(_genre IN $genres WHERE exists((g)-[:HAS_GENRE]->(:Genre {name: _genre}))))
                AND ($developers IS NULL OR size($developers) = 0 OR all(_dev IN $developers WHERE exists((g)-[:DEVELOPED_BY]->(:Developer {name: _dev}))))
                AND ($mechanics IS NULL OR size($mechanics) = 0 OR all(_mech IN $mechanics WHERE exists((g)-[:HAS_MECHANIC]->(:Mechanic {name: _mech}))))

            OPTIONAL MATCH (g)-[:HAS_GENRE]->(gen:Genre)
            OPTIONAL MATCH (g)-[:DEVELOPED_BY]->(dev:Developer)
            OPTIONAL MATCH (g)-[:HAS_MECHANIC]->(mech:Mechanic)

            WITH g, collect(DISTINCT gen.name) AS genres, 
                 collect(DISTINCT dev.name) AS developers, 
                 collect(DISTINCT mech.name) AS mechanics

            RETURN g { .id, .title, .releaseYear, .about, .imagePath } AS game,
                   genres, developers, mechanics
            ORDER BY game.title ASC
            LIMIT $limit";

        var parameters = new
        {
            searchText,
            genres = genres ?? new List<string>(),
            developers = developers ?? new List<string>(),
            mechanics = mechanics ?? new List<string>(),
            limit = maxResults ?? 30
        };

        var cursor = await session.RunAsync(cypher, parameters);
        var records = await cursor.ToListAsync();

        return records.Select(r =>
        {
            var gameDict = r["game"].As<IDictionary<string, object>>();
            return new Game
            {
                Id = gameDict["id"].As<int>(),
                Title = gameDict["title"].As<string>(),
                ReleaseYear = gameDict["releaseYear"].As<int>(),
                About = gameDict.ContainsKey("about") ? gameDict["about"].As<string?>() : null,
                ImagePath = gameDict.ContainsKey("imagePath") ? gameDict["imagePath"].As<string?>() : null,
                Genres = r["genres"].As<List<string>>(),
                Developers = r["developers"].As<List<string>>(),
                Mechanics = r["mechanics"].As<List<string>>()
            };
        }).ToList();
    }

    public async Task<Game?> GetGameByIdAsync(int id)
    {
        await using var session = _neo4j.Driver.AsyncSession();

        var cypher = @"
            MATCH (g:Game {id: $id})
            OPTIONAL MATCH (g)-[:HAS_GENRE]->(gen:Genre)
            OPTIONAL MATCH (g)-[:DEVELOPED_BY]->(dev:Developer)
            OPTIONAL MATCH (g)-[:HAS_MECHANIC]->(mech:Mechanic)

            RETURN g { .id, .title, .releaseYear, .about, .imagePath } AS game,
                   collect(DISTINCT gen.name) AS genres,
                   collect(DISTINCT dev.name) AS developers,
                   collect(DISTINCT mech.name) AS mechanics";

        var cursor = await session.RunAsync(cypher, new { id });
        if (await cursor.PeekAsync() == null) return null;

        var record = await cursor.SingleAsync();
        var gameDict = record["game"].As<IDictionary<string, object>>();

        return new Game
        {
            Id = gameDict["id"].As<int>(),
            Title = gameDict["title"].As<string>(),
            ReleaseYear = gameDict["releaseYear"].As<int>(),
            About = gameDict.ContainsKey("about") ? gameDict["about"].As<string?>() : null,
            ImagePath = gameDict.ContainsKey("imagePath") ? gameDict["imagePath"].As<string?>() : null,
            Genres = record["genres"].As<List<string>>(),
            Developers = record["developers"].As<List<string>>(),
            Mechanics = record["mechanics"].As<List<string>>()
        };
    }

    public async Task<List<Game>> GetRecommendationsAsync(string username)
    {
        await using var session = _neo4j.Driver.AsyncSession();

        var cypher = @"
            MATCH (u:User {username: $username})-[:LIKES]->(liked:Game)
            // Putanja: Korisnik -> Lajkovana igra -> Osobina <- Potencijalna preporuka
            MATCH (liked)-[r:HAS_GENRE|HAS_MECHANIC|DEVELOPED_BY]->(feature)<-[r2:HAS_GENRE|HAS_MECHANIC|DEVELOPED_BY]-(recommended:Game)
            WHERE NOT (u)-[:LIKES]->(recommended)

            WITH recommended, 
                 sum(CASE 
                    WHEN type(r) = 'HAS_MECHANIC' THEN 3
                    WHEN type(r) = 'DEVELOPED_BY'  THEN 2
                    WHEN type(r) = 'HAS_GENRE'     THEN 1
                    ELSE 1 
                 END) AS similarityScore

            ORDER BY similarityScore DESC, recommended.title ASC
            LIMIT 10

            OPTIONAL MATCH (recommended)-[:HAS_GENRE]->(gen:Genre)
            OPTIONAL MATCH (recommended)-[:DEVELOPED_BY]->(dev:Developer)
            OPTIONAL MATCH (recommended)-[:HAS_MECHANIC]->(mech:Mechanic)

            RETURN recommended { .id, .title, .releaseYear, .about, .imagePath } AS game,
                   collect(DISTINCT gen.name) AS genres,
                   collect(DISTINCT dev.name) AS developers,
                   collect(DISTINCT mech.name) AS mechanics,
                   similarityScore";

        var cursor = await session.RunAsync(cypher, new { username });
        var records = await cursor.ToListAsync();

        return records.Select(r =>
        {
            var gameDict = r["game"].As<IDictionary<string, object>>();
            int? sim = r.Keys.Contains("similarityScore") ? (int?)r["similarityScore"].As<int>() : null;
            return new Game
            {
                Id = gameDict["id"].As<int>(),
                Title = gameDict["title"].As<string>(),
                ReleaseYear = gameDict["releaseYear"].As<int>(),
                About = gameDict.ContainsKey("about") ? gameDict["about"].As<string?>() : null,
                ImagePath = gameDict.ContainsKey("imagePath") ? gameDict["imagePath"].As<string?>() : null,
                Genres = r["genres"].As<List<string>>(),
                Developers = r["developers"].As<List<string>>(),
                Mechanics = r["mechanics"].As<List<string>>(),
                SimilarityScore = sim
            };
        }).ToList();
    }

        public async Task<List<Game>> GetAllGamesAsync(int? maxResults = 50)
    {
        await using var session = _neo4j.Driver.AsyncSession();

        var cypher = @"
            MATCH (g:Game)
            OPTIONAL MATCH (g)-[:HAS_GENRE]->(gen:Genre)
            OPTIONAL MATCH (g)-[:DEVELOPED_BY]->(dev:Developer)
            OPTIONAL MATCH (g)-[:HAS_MECHANIC]->(mech:Mechanic)

            RETURN g { .id, .title, .releaseYear, .about, .imagePath } AS game,
                collect(DISTINCT gen.name) AS genres,
                collect(DISTINCT dev.name) AS developers,
                collect(DISTINCT mech.name) AS mechanics
            ORDER BY game.title ASC
            LIMIT $limit";

        var records = await session.RunAsync(cypher, new { limit = maxResults ?? 50 });
        var results = await records.ToListAsync();

        return results.Select(r => MapGameRecord(r)).ToList();
    }

    // Helper metoda za mapiranje 
    private Game MapGameRecord(IRecord record)
    {
        var gameDict = record["game"].As<IDictionary<string, object>>(); // uzima kolonu game i tretira je kao recnik

        return new Game
        {
            Id = gameDict["id"].As<int>(), // konvertuje id iz recnika u int
            Title = gameDict["title"].As<string>(),
            ReleaseYear = gameDict["releaseYear"].As<int>(),
            About = gameDict.ContainsKey("about") ? gameDict["about"].As<string?>() : null,
            ImagePath = gameDict.ContainsKey("imagePath") ? gameDict["imagePath"].As<string?>() : null,
            Genres = record["genres"].As<List<string>>(),
            Developers = record["developers"].As<List<string>>(),
            Mechanics = record["mechanics"].As<List<string>>()
        };
    }

    public async Task<Game> CreateGameAsync(Game game, string developerName, List<string> genreNames, List<string> mechanicNames, string performedByUsername)
    {
        await using var session = _neo4j.Driver.AsyncSession();

        // Provera admin prava
        var isAdmin = await IsAdminAsync(session, performedByUsername);
        if (!isAdmin) throw new UnauthorizedAccessException("Samo admin može dodavati igre");

        var cypher = @"
            // Kreiranje ili pronalaženje developera
            MERGE (dev:Developer {name: $developerName})

            // Kreiranje igre (ako ne postoji po id-u)
            MERGE (g:Game {id: $gameId})
            SET g.title = $title,
                g.releaseYear = $releaseYear,
                g.about = $about,
                g.imagePath = $imagePath

            // Brisanje starih veza ako postoje (za slučaj update-a)
            WITH g, dev
            OPTIONAL MATCH (g)-[r:DEVELOPED_BY|HAS_GENRE|HAS_MECHANIC]->() 
            DELETE r

            // Nova veza sa developerom
            MERGE (g)-[:DEVELOPED_BY]->(dev)

            // Žanrovi
            FOREACH (genreName IN $genreNames |
                MERGE (gen:Genre {name: genreName})
                MERGE (g)-[:HAS_GENRE]->(gen)
            )

            // Mehanike
            FOREACH (mechName IN $mechanicNames |
                MERGE (mech:Mechanic {name: mechName})
                MERGE (g)-[:HAS_MECHANIC]->(mech)
            )

            // Cypher requires a WITH between FOREACH and subsequent MATCH
            WITH g

            OPTIONAL MATCH (g)-[:HAS_GENRE]->(gen:Genre)
            OPTIONAL MATCH (g)-[:DEVELOPED_BY]->(dev2:Developer)
            OPTIONAL MATCH (g)-[:HAS_MECHANIC]->(mech:Mechanic)

            WITH g, collect(DISTINCT gen.name) AS genres, collect(DISTINCT dev2.name) AS developers, collect(DISTINCT mech.name) AS mechanics

            RETURN g { .id, .title, .releaseYear, .about, .imagePath } AS game,
                   genres, developers, mechanics
            LIMIT 1";

        // normalizacija putanje do slike
        string? normalizedImagePath = NormalizeImagePath(game.ImagePath);

        var parameters = new
        {
            gameId = game.Id,
            title = game.Title,
            releaseYear = game.ReleaseYear,
            about = game.About,
            imagePath = normalizedImagePath,
            developerName,
            genreNames,
            mechanicNames
        };

        try
        {
            var cursor = await session.RunAsync(cypher, parameters);
            var record = await cursor.SingleAsync();
            return MapGameRecord(record); 
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"CreateGameAsync error: {ex.Message}");
            Console.Error.WriteLine($"Cypher (truncated): {cypher.Substring(0, Math.Min(cypher.Length, 400))}");
            try { Console.Error.WriteLine($"Parameters: {System.Text.Json.JsonSerializer.Serialize(parameters)}"); } catch { /* ignore serialization errors */ }
            throw;
        }
    }

    public async Task<Game?> UpdateGameAsync(int id, Game updatedGame, string performedByUsername)
    {
        var gameToUpdate = new Game
        {
            Id = id,
            Title = updatedGame.Title,
            ReleaseYear = updatedGame.ReleaseYear,
            About = updatedGame.About,
            ImagePath = updatedGame.ImagePath,
            // Genres, Developers, Mechanics se ne šalju u update-u, već posebno
        };

        // Za update ćemo verovatno slati i nove liste žanrova/mehanika, pa možeš proširiti parametre
        var devName = updatedGame.Developers?.FirstOrDefault() ?? string.Empty;
        var genres = updatedGame.Genres ?? new List<string>();
        var mechs = updatedGame.Mechanics ?? new List<string>();

        // Pozovemo CreateGameAsync koji će MERGE-ovati igru i ažurirati veze
        return await CreateGameAsync(gameToUpdate, devName, genres, mechs, performedByUsername);
    }

    public async Task<bool> DeleteGameAsync(int id, string performedByUsername)
    {
        await using var session = _neo4j.Driver.AsyncSession();

        if (!await IsAdminAsync(session, performedByUsername))
            throw new UnauthorizedAccessException("Samo admin može brisati igre");

        var cypher = @"
            MATCH (g:Game {id: $id})
            DETACH DELETE g";

        await session.RunAsync(cypher, new { id });
        return true;
    }

    // "čistač" (normalizator) putanja do slika
    private string? NormalizeImagePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        var p = path.Trim();

        //proverava da li je pun URL (sa http). Ako jeste, uzima samo "lokalni" deo
        // https://mojsajt.com/Images/Avatar.png -> /Images/Avatar.png
        if (Uri.TryCreate(p, UriKind.Absolute, out var uri)) p = uri.AbsolutePath;

        // pretvara /Images u /images
        if (p.StartsWith("/Images", StringComparison.OrdinalIgnoreCase))
        {
            p = "/images" + p.Substring(p.IndexOf('/', 1));
        }

        // Osigurava da putanja uvek počinje sa kosom crtom /
        if (!p.StartsWith('/')) p = "/" + p;

        return p;
    }

    // metoda za proveru admina
    private async Task<bool> IsAdminAsync(IAsyncSession session, string username)
    {
        var result = await session.RunAsync(@"
            MATCH (u:User {username: $username})
            RETURN u.role = 'admin' AS isAdmin",
            new { username });

        var records = await result.ToListAsync();

        // Ako nema korisnika → false
        // Ako ima → uzimamo prvu vrednost
        return records.Count > 0 && records[0]["isAdmin"].As<bool>();
    }
}