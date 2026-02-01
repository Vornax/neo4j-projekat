/**
 * state.js
 * Upravljanje stanjem aplikacije i lokalnim podacima
 */

// Centralno stanje aplikacije
export let appState = {
    currentUser: null,   // Trenutno izabrani korisnik (npr. 'luka', 'admin', 'guest')
    favorites: [],       // Niz ID-jeva igara koje je korisnik lajkovao
    games: [],           // Igre koje su trenutno filtrirane/prikazane
    allGames: []         // Kompletna lista igara preuzeta sa servera
};


// Generiše ključ za localStorage na osnovu trenutnog korisnika
export function getUserKey() {
    const user = appState.currentUser || 'user';
    return `favorites_${user}`;
}

// Učitava listu lajkova iz localStorage-a ako API nije dostupan
export function loadFavoritesFromStorage() {
    try {
        const raw = localStorage.getItem(getUserKey());
        appState.favorites = raw ? JSON.parse(raw) : [];
    } catch (e) {
        console.warn("Greška pri učitavanju favorita:", e);
        appState.favorites = [];
    }
}

// Snima trenutne favorite u localStorage
export function saveFavoritesToStorage() {
    localStorage.setItem(getUserKey(), JSON.stringify(appState.favorites));
}

// Pomoćna funkcija za resetovanje stanja igara
export function setGames(newGames) {
    appState.games = [...newGames];
}

export function setAllGames(newGames) {
    appState.allGames = [...newGames];
    appState.games = [...newGames];
}