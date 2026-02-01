using GameRecommendationApi.Services;
using GameRecommendationApi.Services.Interfaces;
using GameRecommendationApi.Middleware;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Učitaj .env fajl
DotNetEnv.Env.Load();

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// kada kontroleru zatreba neko da obradi zahtev (rad sa bazom), na osnovu ovg spiska zna kog da zove
builder.Services.AddSingleton<Neo4jService>();
builder.Services.AddScoped<IMetadataService, MetadataService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGameService, GameService>(); // AddScoped Napravi se nova kopija klase za svaki novi zahtev(request)

builder.Services.AddCors(options =>
{
    options.AddPolicy("CORS", policy =>
    {
        policy.WithOrigins( new string[]{
            "http://localhost:5500",
            "https://localhost:5500",
            "http://127.0.0.1:5500",
            "https://127.0.0.1:5500"
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Serve frontend images from /images route (physical folder Frontend/Images)
var imagesPath = Path.Combine(app.Environment.ContentRootPath, "Frontend", "Images");
if (Directory.Exists(imagesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imagesPath),
        RequestPath = "/images"
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CORS");
app.UseAuthorization();
// Registruj Authorization Middleware
app.UseMiddleware<AuthorizationMiddleware>();
app.MapControllers();
app.Run();

