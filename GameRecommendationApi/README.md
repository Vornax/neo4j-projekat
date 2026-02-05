
1. Kloniraj i uđi u direktorijum
  git clone <repo-url>
  cd GameRecommendationApi

2. Podigni kontejnere (safe — kreira volume i izvrši seed ako je data-folder prazan)
  docker compose pull
  docker compose up -d

3. Sačekaj da Neo4j bude spreman (čekaj dok log ne pokaže Started) i proveri seed:
  docker compose logs neo4j --tail 50 --follow

4. ako je baza prazna — eksplicitno pokreni seed skriptu 
type .\neo4j-seed\01-seed.cypher | docker exec -i game-rec-neo4j bin/cypher-shell -u neo4j -p 'sifra'

5. Verifikuj da je DB popunjena (očekivano: 50)
  docker exec -i game-rec-neo4j bin/cypher-shell -u neo4j -p 'sifra' "MATCH (g:Game) RETURN count(g) AS games;"

//Očekivani izlaz: games = 50

Ako nakon pokretanja baza ostane PRAZNA — brzo dijagnostičke komande 🩺

-Da li postoji volume?
  docker volume ls | Select-String neo4j

-Da li seed nije pokrenut / ima grešaka u logu?
  docker compose logs neo4j --tail 200

Ponovno seedovanje (sigurno):
  type .\neo4j-seed\01-seed.cypher | docker exec -i game-rec-neo4j bin/cypher-shell -u neo4j -p 'sifra'