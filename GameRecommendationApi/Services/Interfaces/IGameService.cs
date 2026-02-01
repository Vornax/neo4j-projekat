using GameRecommendationApi.Models;

namespace GameRecommendationApi.Services.Interfaces;

public interface IGameService
{
    Task<List<Game>> SearchGamesAsync(string? searchText, List<string>? genres, List<string>? developers, List<string>? mechanics, int? maxResults = 30);     // pretraga po parametrima
    Task<Game?> GetGameByIdAsync(int id);     // vraca igru po id
    Task<List<Game>> GetRecommendationsAsync(string username); // vraca preporuke
    Task<List<Game>> GetAllGamesAsync(int? maxResults = 50); // vraca sve igre
    Task<Game> CreateGameAsync(Game game, string developerName, List<string> genreNames, List<string> mechanicNames,  string performedByUsername); //  kreira igru 
    Task<Game?> UpdateGameAsync(int id, Game updatedGame, string performedByUsername); // azuriranje igre
    Task<bool> DeleteGameAsync(int id, string performedByUsername); // brisanej igre 
}