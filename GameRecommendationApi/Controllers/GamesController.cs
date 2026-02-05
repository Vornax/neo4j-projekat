using GameRecommendationApi.Models;
using GameRecommendationApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameRecommendationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IUserService _userService;
    private readonly IMetadataService _metadataService;

    public GamesController(IGameService gameService, IUserService userService, IMetadataService metadataService)
    {
        _gameService = gameService;
        _userService = userService;
        _metadataService = metadataService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<Game>>> Search(
        [FromQuery] string? searchText = null,
        [FromQuery] List<string>? genres = null,
        [FromQuery] List<string>? developers = null,
        [FromQuery] List<string>? mechanics = null,
        [FromQuery] int? maxResults = 30)
    {
        var results = await _gameService.SearchGamesAsync(
            searchText, genres, developers, mechanics, maxResults);

        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetGame(int id)
    {
        var game = await _gameService.GetGameByIdAsync(id);
        return game != null ? Ok(game) : NotFound();
    }

    [HttpGet("recommendations/{username}")]
    public async Task<ActionResult<List<Game>>> GetRecommendations(string username)
    {
        var recs = await _gameService.GetRecommendationsAsync(username);
        return Ok(recs);
    }

    [HttpGet("filters")]
    public async Task<ActionResult> GetFilters()
    {
        var genres = await _metadataService.GetAllGenresAsync();
        var mechanics = await _metadataService.GetAllMechanicsAsync();
        var developers = await _metadataService.GetAllDevelopersAsync();
        return Ok(new { genres, mechanics, developers });
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<Game>>> GetAll([FromQuery] int? maxResults = 1000)
    {
        var games = await _gameService.GetAllGamesAsync(maxResults);
        return Ok(games);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{username}/likes")]
    public async Task<ActionResult<List<int>>> GetUserLikes(string username)
    {
        var likes = await _userService.GetUserLikesAsync(username);
        return Ok(likes);
    }

    [HttpPost]
    [Route("create")]
    public async Task<ActionResult<Game>> CreateGame(
        [FromBody] GameCreateRequest request,
        [FromQuery] string performedBy)  // username koji izvršava akciju
    {
        var created = await _gameService.CreateGameAsync(
            request.Game,
            request.DeveloperName,
            request.GenreNames,
            request.MechanicNames,
            performedBy);

        return CreatedAtAction(nameof(GetGame), new { id = created.Id }, created);
    }

    public class GameCreateRequest
    {
        public Game Game { get; set; } = new();
        public string DeveloperName { get; set; } = "";
        public List<string> GenreNames { get; set; } = new();
        public List<string> MechanicNames { get; set; } = new();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Game>> UpdateGame(int id, [FromBody] Game updatedGame, [FromQuery] string performedBy)
    {
        try
        {
            var updated = await _gameService.UpdateGameAsync(id, updatedGame, performedBy);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (Exception ex)
        {
            // Log server-side for debugging
            Console.Error.WriteLine($"Error updating game {id}: {ex}");
            // Return a Problem response with a concise message (avoid leaking stacktrace in production)
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(int id, [FromQuery] string performedBy)
    {
        await _gameService.DeleteGameAsync(id, performedBy);
        return NoContent();
    }

    [HttpPost("wishlist/{gameId}")]
    public async Task<IActionResult> AddToWishlist([FromQuery] string username, int gameId)
    {
        var success = await _userService.AddToWishlistAsync(username, gameId);
        return success ? Ok() : BadRequest("Neuspešno dodavanje u wishlist");
    }

    [HttpDelete("wishlist/{gameId}")]
    public async Task<IActionResult> RemoveFromWishlist([FromQuery] string username, int gameId)
    {
        var success = await _userService.RemoveFromWishlistAsync(username, gameId);
        return success ? Ok() : BadRequest("Neuspešno uklanjanje iz wishlist");
    }
}