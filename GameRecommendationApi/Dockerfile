# ────────────────────────────────────────────────────────────────
# Faza 1 – iskompajlira projekat i pravi gotovu verziju aplikacije
# ────────────────────────────────────────────────────────────────

# skida image dotneta
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build 

# Napravi folder /src unutar kontejnera i uđi u njega 
WORKDIR /src

# Kopiramo samo .csproj fajlove sa kompjutera u kontejner, da bi iz tih fajlova Docker mogao da vidi koje pakete treba da skine 
COPY *.csproj .

# skida sve NuGet pakete (biblioteke) koje projektu trebaju 
RUN dotnet restore

# Kopiramo sav ostali kod iz foldera u kontejner
COPY . .

# Kompajlira kod za produkciju(Release), spakuje ga i fajlove u folder /app/publish. 
# Opcija --no-restore kaže: "Ne skidaj pakete opet, već smo to uradili iznad"
RUN dotnet publish -c Release -o /app/publish --no-restore

# ────────────────────────────────────────────────────────────────
# Faza 2 – pravi mali finalni image koji samo pokreće aplikaciju
# ────────────────────────────────────────────────────────────────

# Docker počinje novi image koji je baziran na ASP.NET runtime image-u (koji je manji od SDK image-a)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Napravi folder /app unutar kontejnera i uđi u njega 
WORKDIR /app

# uzali u img bild u njen /app/publish i kopira sve fajlove u ovaj img final u folder /app
COPY --from=build /app/publish .

# uzali u img bild u njen /src/Frontend i kopira sve fajlove u ovaj img final u folder /app/Frontend
COPY --from=build /src/Frontend ./Frontend

# otvara port 5257, koji je port na kojem će aplikacija slušati unutar kontejnera
EXPOSE 5257

# postavlja environment na "Development", što omogućava detaljnije greške 
ENV ASPNETCORE_ENVIRONMENT=Development

# Kažemo Docker-u koja komanda treba da se pokrene kad se kontejner startuje
ENTRYPOINT ["dotnet", "GameRecommendationApi.dll"]