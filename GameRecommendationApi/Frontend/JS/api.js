/**
 * api.js
 * Svi mrežni zahtevi ka backend serveru
 */

import { API_BASE, normalizeImagePath, apiFetch } from './utils.js';


//Preuzima sve igre sa servera
export async function fetchAllGames() {
    try {
        const res = await apiFetch(`${API_BASE}/all`);
        if (!res.ok) throw new Error('Mrežna greška pri dobijanju svih igara');
        const data = await res.json();
        return data.map(g => normalizeImagePath(g));
    } catch (e) {
        console.warn('Greška pri API pozivu, koriste se mock podaci.', e);
        return null; 
    }
}


// Pretraga igara na serveru na osnovu teksta i filtera

export async function searchGamesAPI(searchText, filters) {
    const params = new URLSearchParams();
    if (searchText) params.append('searchText', searchText);
    
    filters.genres.forEach(g => params.append('genres', g));
    filters.mechanics.forEach(m => params.append('mechanics', m));
    filters.developers.forEach(d => params.append('developers', d));
    
    params.append('maxResults', '200');

    try {
        const res = await apiFetch(`${API_BASE}/search?` + params.toString());
        if (!res.ok) throw new Error('Pretraga nije uspela');
        const data = await res.json();
        return data.map(g => normalizeImagePath(g));
    } catch (e) {
        console.error('API pretraga nije uspela:', e);
        throw e; 
    }
}


// Preuzima preporuke za određenog korisnika.
export async function fetchRecommendationsAPI(username) {
    try {
        const res = await apiFetch(`${API_BASE}/recommendations/` + encodeURIComponent(username));
        if (!res.ok) throw new Error('Neuspešno učitavanje preporuka');
        const data = await res.json();
        return data.map(g => normalizeImagePath(g));
    } catch (e) {
        console.warn('Preporuke API error:', e);
        return null;
    }
}


// Preuzima listu ID-jeva igara koje je korisnik lajkovao.
export async function fetchUserLikes(username) {
    try {
        const res = await apiFetch(`${API_BASE}/users/${encodeURIComponent(username)}/likes`);
        if (!res.ok) throw new Error('Neuspešno učitavanje lajkova');
        const ids = await res.json();
        return ids.map(id => Number(id));
    } catch (e) {
        console.error('Greška pri učitavanju lajkova sa servera:', e);
        return null;
    }
}


// Dodaje ili uklanja igru iz wishlist-e na serveru.
export async function toggleWishlistAPI(gameId, username, isAdding = true) {
    const method = isAdding ? 'POST' : 'DELETE';
    try {
        const res = await apiFetch(`${API_BASE}/wishlist/${gameId}?username=${encodeURIComponent(username)}`, { method });
        return res.ok;
    } catch (e) {
        console.error('Wishlist API error:', e);
        return false;
    }
}


// Admin funkcija za brisanje igre.
export async function deleteGameAPI(gameId, performedBy) {
    try {
        const res = await apiFetch(`${API_BASE}/${gameId}?performedBy=${encodeURIComponent(performedBy)}`, { method: 'DELETE' });
        return res.ok;
    } catch (e) {
        console.error('Delete API error:', e);
        return false;
    }
}


// Preuzima dostupne filtere (žanrove, mehanike, developere).
export async function fetchFiltersAPI() {
    try {
        const res = await apiFetch(`${API_BASE}/filters`);
        if (!res.ok) throw new Error('Neuspešno učitavanje filtera');
        return await res.json();
    } catch (e) {
        console.warn('Filteri API error:', e);
        return null;
    }
}


// Preuzima listu korisnika za prebacivanje profila.
export async function fetchUsersAPI() {
    try {
        const res = await apiFetch(`${API_BASE}/users`);
        if (!res.ok) throw new Error('Neuspešno učitavanje korisnika');
        return await res.json();
    } catch (e) {
        console.error('Greška pri učitavanju korisnika:', e);
        return [];
    }
}