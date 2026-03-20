
Ovaj projekat koristi Docker okruženje kako bi se baza podataka (Neo4j), Backend API (.NET 8) i Frontend pokrenuli brzo.

🛠️ Preduslovi za pokretanje

Da biste pokrenuli projekat, potrebno je da imate instalirano:

- Docker Desktop (upaljen i aktivan u pozadini)
- Git (za preuzimanje repozitorijuma)


🚀 Uputstvo za pokretanje 


1. Kloniranje repozitorijuma i pozicioniranje u folder

Otvorite terminal (Command Prompt, PowerShell ili Git Bash) i unesite:

git clone <link_do_github_repozitorijuma>
cd <ime_foldera>

2. Podešavanje promenljivih okruženja (.env)

Projekat koristi .env fajl za čuvanje kredencijala baze i API ključeva, koji iz bezbednosnih razloga nije na GitHub-u.

Pronađite fajl koji se zove .env.example.
Napravite kopiju tog fajla i preimenujte je u .env
(Opciono) Otvorite .env i izmenite šifre po želji, ili jednostavno ostavite defaultne vrednosti koje su već tu pripremljene za testiranje.

3. Pokretanje Docker kontejnera

U terminalu (u root folderu projekta gde se nalazi docker-compose.yml), ukucajte sledeću komandu:
docker-compose up -d --build

4. Pristup aplikaciji

Kada terminal završi proces, celokupan sistem je aktivan! Otvorite vaš web pretraživač i posetite sledeće linkove:

- 🌐 Glavni Web Sajt (Frontend): Frontend > index.html > desni klik > open with live server
- ⚙️ API Dokumentacija (Swagger): http://localhost:5257/swagger 
- 🗄️ Baza Podataka (Neo4j Browser): http://localhost:7474
(Za autorizaciju na swagger koristiti podatak iz .env AUTHORIZATION_APIKEY)


!!! da bi sve radilo u .env polja AUTHORIZATION_APIKEY i Authorization__ApiKey moraju da sadrze istu sifru kao i promenjiva API_KEY u utils.js !!!



