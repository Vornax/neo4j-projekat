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

    // lightweight SVG data-URI placeholder (works offline)
    const DATA_PLACEHOLDER = "data:image/svg+xml;utf8," + encodeURIComponent(
        "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 220 300'>" +
        "<rect width='100%' height='100%' fill='%23efefef'/>" +
        "<text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='%23666' font-family='Arial,Helvetica,sans-serif' font-size='20'>No Image</text>" +
        "</svg>"
    );

    let raw = (game.imagePath || game.image || '').toString().trim();

    // empty / explicit null/undefined strings -> use placeholder
    if (!raw || /^\s*(undefined|null)\s*$/i.test(raw)) {
        game.image = DATA_PLACEHOLDER;
        return game;
    }

    // If it's already an absolute URL, keep it exactly as provided (do NOT prepend '/').
    if (/^https?:\/\//i.test(raw)) {
        game.imagePath = raw;
        game.image = raw;
        if (game.about && !game.description) game.description = game.about;
        return game;
    }

    // Normalize relative/local paths (remove extra leading slashes)
    const rawNoLeading = raw.replace(/^\/*/, '');
    let path;

    if (!rawNoLeading.includes('/')) {
        path = `/images/${rawNoLeading}`;
    } else {
        path = '/' + rawNoLeading;
        path = path.replace(/^\/Images/i, '/images');
    }

    // If path points to local /images folder, prefix API host
    if (path.toLowerCase().startsWith('/images')) {
        const apiHost = API_BASE.replace(/\/api\/Games\/?$/i, '');
        game.imagePath = `${apiHost}${path}`;
    } else {
        game.imagePath = path;
    }

    game.image = game.imagePath || DATA_PLACEHOLDER;
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