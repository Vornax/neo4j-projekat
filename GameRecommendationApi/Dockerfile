# ────────────────────────────────────────────────────────────────
# Faza 1 – iskompajlira projekat i pravi gotovu verziju aplikacije
# ────────────────────────────────────────────────────────────────

# uzimamo startni image dotneta
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build 

# Postavljamo radni direktorijum unutar imiga
WORKDIR /src

# Kopiramo samo .csproj fajlove, ako u src
COPY *.csproj .

# ova linija procita .cprosj i iz njih vidi koji su paketi potrebni i skine ih
RUN dotnet restore

# Kopiramo sav ostali kod
COPY . .

# Docker privremeno pokrene mini računar i ukuca ovu komandu u terminal
# publish  - pravi verziju aplikacije koja može odmah da se pokrene
# -o = output folder /app/publish unutar image-a
# --no-restore = ne radi ponovo restore jer smo to već uradili
RUN dotnet publish -c Release -o /app/publish --no-restore

# ────────────────────────────────────────────────────────────────
# Faza 2 – pravi mali finalni image koji samo pokreće aplikaciju
# ────────────────────────────────────────────────────────────────

# Docker počinje novi image od nule, ali koristi sliku sa dotnet runtime-om
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# pravi folder UNUTAR runtime image-a
WORKDIR /app

# uzali u img bild u njen /app/publish i kopira sve fajlove u ovaj img final
COPY --from=build /app/publish .

# Ensure frontend static assets are present in the final image (so /images works)
COPY --from=build /src/Frontend ./Frontend

# Izlažemo port na kome sluša aplikacija 
EXPOSE 5257

# Postavljamo environment varijablu da aplikacija zna da radi u Dockeru
# (možeš koristiti za različito ponašanje ako želiš)
ENV ASPNETCORE_ENVIRONMENT=Development

# Kažemo Docker-u koja komanda treba da se pokrene kad se kontejner startuje
ENTRYPOINT ["dotnet", "GameRecommendationApi.dll"]