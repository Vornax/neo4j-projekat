using GameRecommendationApi.Models;

namespace GameRecommendationApi.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync(); // vrati sve korisnike
    Task<List<int>> GetUserLikesAsync(string username); // vrati listu ID-jeva igara koje korisnik voli
    Task<bool> AddToWishlistAsync(string username, int gameId); // dodavanje u listu svidjanja
    Task<bool> RemoveFromWishlistAsync(string username, int gameId); // uklanjanje iz liste svidjanja
}