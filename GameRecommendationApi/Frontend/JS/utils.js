/**
 * utils.js
 * Pomoćne funkcije za obradu podataka, filtriranje i performanse
 */

// Konstanta za API bazu koju koriste i drugi moduli
export const API_BASE = 'http://localhost:5257/api/Games';
export const API_KEY = 'dev-secret-key-123';


// Fetch helper funkcija koja automatski dodaje Authorization header
export async function apiFetch(url, options = {}) {
    const headers = {
        ...options.headers,
        'Authorization': API_KEY
    };
    
    return fetch(url, { ...options, headers });
}


// Debounce utility: Sprečava prečesto izvršavanje funkcije (npr. pri kucanju u search bar)
export function debounce(fn, delay = 300) {
    let t;
    return (...args) => {
        clearTimeout(t);
        t = setTimeout(() => fn(...args), delay);
    };
}


// Normalizacija putanje slike: Osigurava da svaka igra ima ispravan URL za sliku,
// bilo da je u pitanju lokalni fajl na serveru ili eksterni link.
export function normalizeImagePath(game) {
    if (!game) return game;
    
    let raw = game.imagePath || game.image || null;

    // Provera da li je putanja prazna ili sadrži string "undefined"/"null"
    if (typeof raw === 'string' && (/^\s*undefined\s*$/i.test(raw) || /^\s*null\s*$/i.test(raw))) {
        raw = null;
    }
    
    if (!raw) {
        game.image = 'https://via.placeholder.com/220x300?text=No+Image';
        return game;
    }

    // Normalizacija kosing crta i foldera Images -> images
    const rawNoLeading = String(raw).replace(/^\/*/, '');
    let path;
    
    if (!rawNoLeading.includes('/')) {
        path = `/images/${rawNoLeading}`;
    } else {
        path = String(raw).replace(/^\/*/, '/');
        path = path.replace(/^\/Images/i, '/images');
    }

    // Ako putanja vodi do lokalnog foldera, dodajemo host API servera
    if (path.toLowerCase().startsWith('/images')) {
        const apiHost = API_BASE.replace(/\/api\/Games\/?$/i, '');
        game.imagePath = `${apiHost}${path}`;
    } else if (!/^https?:\/\//i.test(path)) {
        game.imagePath = path;
    } else {
        game.imagePath = path;
    }

    // Postavljanje finalne image osobine koju renderer koristi
    game.image = game.imagePath || 'https://via.placeholder.com/220x300?text=No+Image';

    // Kompatibilnost unazad: ako server vrati 'about', a frontend očekuje 'description'
    if (game.about && !game.description) game.description = game.about;

    return game;
}


// Prikuplja sve selektovane vrednosti iz filter checkbox-ova
export function getSelectedFilters() {
    const checked = document.querySelectorAll('.filter-checkbox:checked');
    const filters = { genres: [], mechanics: [], developers: [] };
    
    checked.forEach(cb => {
        const t = cb.getAttribute('data-type');
        if (t === 'genre') filters.genres.push(cb.value);
        if (t === 'mechanic') filters.mechanics.push(cb.value);
        if (t === 'developer') filters.developers.push(cb.value);
    });
    
    return filters;
}


// Klijentsko filtriranje igara (koristi se kao fallback ako API search ne uspe)
export function filterGamesLocally(gamesList, searchText, filters) {
    const search = (searchText || '').toLowerCase().trim();
    
    return gamesList.filter(g => {
        // Search provera
        if (search) {
            const match = g.title.toLowerCase().includes(search) || 
                         (g.description || '').toLowerCase().includes(search);
            if (!match) return false;
        }
        
        // Filteri provera
        if (filters.genres.length && !filters.genres.every(f => g.genres.includes(f))) return false;
        if (filters.mechanics.length && !filters.mechanics.every(f => g.mechanics.length && g.mechanics.includes(f))) return false;
        if (filters.developers.length && !filters.developers.includes(g.developer)) return false;
        
        return true;
    });
}