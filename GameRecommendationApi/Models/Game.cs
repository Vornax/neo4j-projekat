namespace GameRecommendationApi.Models;

public class Game
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string? About { get; set; }          
    public string? ImagePath { get; set; }      // putanja do slike (npr. "/images/covers/gta5.jpg")
    public List<string> Genres { get; set; } = new();
    public List<string> Developers { get; set; } = new();
    public List<string> Mechanics { get; set; } = new();
    public int? SimilarityScore { get; set; } = null;
}
