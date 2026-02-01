namespace GameRecommendationApi.Services.Interfaces;

public interface IMetadataService
{
    // Vracanje svih žanrova, mehanika i developera
    Task<List<string>> GetAllGenresAsync();
    Task<List<string>> GetAllMechanicsAsync();
    Task<List<string>> GetAllDevelopersAsync();
}