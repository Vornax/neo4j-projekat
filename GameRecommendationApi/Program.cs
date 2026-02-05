using GameRecommendationApi.Services;
using GameRecommendationApi.Services.Interfaces;
using GameRecommendationApi.Middleware;
using DotNetEnv;
using Microsoft.OpenApi.Models;

// Učitaj .env fajl pre nego što kreiramo IConfiguration — tako će vrednosti iz .env biti vidljive
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// Swagger: dodaj ApiKey security schema tako da Swagger UI može poslati `Authorization` header
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Enter the API key value (no 'Bearer ' prefix)."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" },
                In = ParameterLocation.Header,
                Name = "Authorization"
            },
            new string[] { }
        }
    });
});

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

app.UseMiddleware<AuthorizationMiddleware>();
app.MapControllers();
app.Run();

