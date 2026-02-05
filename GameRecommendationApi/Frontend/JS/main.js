/**
 * main.js
 * fajl koji povezuje logiku, API i UI
 */

import { appState, loadFavoritesFromStorage, saveFavoritesToStorage, getUserKey } from './state.js';
import { fetchAllGames, searchGamesAPI, fetchRecommendationsAPI, toggleWishlistAPI, fetchFiltersAPI, fetchUsersAPI, fetchUserLikes, deleteGameAPI } from './api.js';
import { renderGames, renderFilters, renderAdminTable, renderUserSelect } from './render.js';
import { debounce, getSelectedFilters, filterGamesLocally, API_BASE, apiFetch } from './utils.js';
import { toggleUserAdminView, switchTabUI, openDetailsModal, closeModal, openAdminFormModal } from './ui.js';

// --- Inicijalizacija ---

document.addEventListener('DOMContentLoaded', async () => {
    // 1. Učitaj korisnike i postavi početni mod (koristi saved selection kad postoji)
    const users = await fetchUsersAPI();
    const savedUser = localStorage.getItem('selectedUser');
    const defaultUser = savedUser || 'guest';
    renderUserSelect(users, defaultUser);

    const selectEl = document.getElementById('userMode');
    const available = selectEl ? Array.from(selectEl.options).map(o => o.value) : [];
    const initialUser = available.includes(defaultUser) ? defaultUser : (available[0] || 'guest');
    appState.currentUser = initialUser;

    if (selectEl) selectEl.value = appState.currentUser;
    localStorage.setItem('selectedUser', appState.currentUser);

    toggleUserAdminView(appState.currentUser);

    // 2. Učitaj filtere sa servera
    const filterData = await fetchFiltersAPI();
    if (filterData) renderFilters(filterData);

    // 3. Učitaj sve igre
    const games = await fetchAllGames();
    appState.allGames = games || [];
    appState.games = games || [];

    // 4. Učitaj favorite 
    try {
        const likes = await fetchUserLikes(appState.currentUser);
        if (likes && likes.length) appState.favorites = likes;
        else loadFavoritesFromStorage();
    } catch (e) {
        loadFavoritesFromStorage();
    }

    renderGames(appState.games);
    renderAdminTable(appState.allGames);

    setupEventListeners();
});

// --- Event Listeners ---

function setupEventListeners() {
    // Promena korisnika/admina
    document.getElementById('userMode')?.addEventListener('change', async (e) => {
        const val = e.target.value;
        localStorage.setItem('selectedUser', val);
        appState.currentUser = val;
        // pokušaj da preuzmemo lajkove sa servera
        try {
            const likes = await fetchUserLikes(val);
            if (likes && likes.length) appState.favorites = likes;
            else loadFavoritesFromStorage();
        } catch (err) {
            loadFavoritesFromStorage();
        }
        toggleUserAdminView(appState.currentUser);
        renderGames(appState.games);

        const recs = await fetchRecommendationsAPI(appState.currentUser);
        if (recs) renderGames(recs, 'recommendationsGrid');
    });

    // Navigacija (Tabovi)
    document.querySelectorAll('.nav-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            document.querySelectorAll('.nav-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            
            const tab = btn.getAttribute('data-tab');
            switchTabUI(tab);

            if (tab === 'wishlist') {
                const wishlistGames = appState.allGames.filter(g => appState.favorites.includes(Number(g.id)));
                renderGames(wishlistGames);
            } else if (tab === 'recommendations') {
                const recs = await fetchRecommendationsAPI(appState.currentUser);
                renderGames(recs || [], 'recommendationsGrid');
            } else {
                renderGames(appState.games);
            }
        });
    });

    // Pretraga sa debounce-om (Ceka dok ne prestane kucanje na 400 milisekundi, tek onda poziva zahtev)
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', debounce(async (e) => {
            const text = e.target.value;
            const filters = getSelectedFilters();
            try {
                const results = await searchGamesAPI(text, filters);
                appState.games = results;
            } catch {
                appState.games = filterGamesLocally(appState.allGames, text, filters);
            }
            renderGames(appState.games);
        }, 400));
    }

    // Filteri (checkboxes)
    document.addEventListener('change', async (e) => {
        if (e.target.classList.contains('filter-checkbox')) {
            const text = document.getElementById('searchInput')?.value || '';
            const filters = getSelectedFilters();
            const results = await searchGamesAPI(text, filters);
            appState.games = results;
            renderGames(appState.games);
        }
    });

    // Klik na karticu igre (Detalji i Lajk) 
    document.addEventListener('click', async (e) => {
        const card = e.target.closest('.game-card');
        if (!card) return;
        const gameId = Number(card.dataset.id);
        const game = appState.allGames.find(g => g.id === gameId);
        if (!game) return;

        if (e.target.classList.contains('fav-btn')) {
            e.stopPropagation();
            const isAdding = !appState.favorites.includes(gameId);
            const success = await toggleWishlistAPI(gameId, appState.currentUser, isAdding);
            
            if (success || !success) { // Fallback na lokalno ako API ne uspe
                if (isAdding) appState.favorites.push(gameId);
                else appState.favorites = appState.favorites.filter(id => id !== gameId);
                saveFavoritesToStorage();
                // Osveži trenutno vidljivu listu (store/recs/wishlist)
                const activeTab = document.querySelector('.nav-btn.active')?.getAttribute('data-tab');
                if (activeTab === 'recommendations') {
                    const recs = await fetchRecommendationsAPI(appState.currentUser);
                    renderGames(recs || [], 'recommendationsGrid');
                } else if (activeTab === 'wishlist') {
                    const wishlistGames = appState.allGames.filter(g => appState.favorites.includes(Number(g.id)));
                    renderGames(wishlistGames);
                } else {
                    renderGames(appState.games);
                }
            }
        } else {
            openDetailsModal(game);
            // Postavi wishlist dugme unutar modala
            const wishlistBtn = document.getElementById('wishlistActionBtn');
            if (wishlistBtn) {
                wishlistBtn.dataset.id = game.id;
                const liked = appState.favorites.includes(Number(game.id));
                wishlistBtn.innerText = liked ? 'Ne sviđa mi se' : 'Sviđa mi se';
                wishlistBtn.classList.toggle('liked', liked);
            }

        }
    });

    // klikovi sa desne strane (tagovi)
    document.querySelector('.modal-right')?.addEventListener('click', async (e) => {
        const tag = e.target.closest('.tag');
        // IGNORIŠI klikove tagova koji su unutar modalnih lista žanrova ili mehanika
        if (tag && (tag.closest('#modalGenres') || tag.closest('#modalMechanics'))) {
            return; // ne radimo ništa za tagove unutar modala
        }
        if (tag) {
            const txt = tag.innerText;
 
            const cb = Array.from(document.querySelectorAll('.filter-checkbox')).find(c => c.value === txt);
            if (cb) cb.checked = true;

            const text = document.getElementById('searchInput')?.value || '';
            const filters = getSelectedFilters();
            try {
                const results = await searchGamesAPI(text, filters);
                appState.games = results;
            } catch {
                appState.games = filterGamesLocally(appState.allGames, text, filters);
            }
            renderGames(appState.games);
            closeModal('gameModal');

            document.querySelector('.nav-btn[data-tab="store"]')?.click();
        }
    });

    // Wishlist dugme unutar modala
    document.getElementById('wishlistActionBtn')?.addEventListener('click', async (e) => {
        const id = Number(e.target.dataset.id);
        const isAdding = !appState.favorites.includes(id);
        const ok = await toggleWishlistAPI(id, appState.currentUser, isAdding);
        if (ok || !ok) {
            if (isAdding) appState.favorites.push(id);
            else appState.favorites = appState.favorites.filter(i => i !== id);
            saveFavoritesToStorage();
            // update dugme
            const liked = appState.favorites.includes(id);
            e.target.innerText = liked ? 'Ne sviđa mi se' : 'Sviđa mi se';
            e.target.classList.toggle('liked', liked);
            renderGames(appState.games);
            const recs = await fetchRecommendationsAPI(appState.currentUser);
            if (recs) renderGames(recs, 'recommendationsGrid');
        }
    });

    // Admin tabela: edit / delete
    let deletePendingId = null;
    document.getElementById('adminTableBody')?.addEventListener('click', (e) => {
        if (e.target.classList.contains('edit-game-btn')) {
            const id = Number(e.target.dataset.id);
            const game = appState.allGames.find(g => g.id === id);
            openAdminFormModal('edit', game);
        } else if (e.target.classList.contains('delete-game-btn')) {
            deletePendingId = Number(e.target.dataset.id);
            document.getElementById('confirmDeleteModal')?.classList.remove('hidden');
        }
    });

    // Admin modal submit
    document.getElementById('adminGameForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const form = e.target;
        const mode = form.dataset.mode || 'create';
        const editId = form.dataset.editId;
 
        const title = document.getElementById('adminTitle').value;
        const developer = document.getElementById('adminDeveloper').value;
        const year = parseInt(document.getElementById('adminYear').value) || 0;
        const image = document.getElementById('adminImage').value;
        const genres = document.getElementById('adminGenres').value.split(',').map(s => s.trim()).filter(Boolean);
        const mechanics = document.getElementById('adminMechanics').value.split(',').map(s => s.trim()).filter(Boolean);
        const description = document.getElementById('adminDescription').value;

        const username = appState.currentUser || 'luka';

        if (mode === 'create') {

            const body = { Game: { title, about: description, imagePath: image, releaseYear: year, developers: developer ? [developer] : [] }, DeveloperName: developer, GenreNames: genres, MechanicNames: mechanics };
            const res = await apiFetch(`${API_BASE}/create?performedBy=${encodeURIComponent(username)}`, {
                method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body)
            });
            if (res.ok) {
                closeModal('adminModal');
                const games = await fetchAllGames();
                if (games) { appState.allGames = games; appState.games = games; }
                renderAdminTable(appState.allGames);
                alert('Igra uspješno dodata.');
            } else {
                const text = await res.text();
                console.error('Create error:', text);
                alert('Greška pri dodavanju igre: ' + (text || res.statusText));
            }
        } else {
            const updated = { id: Number(editId), title, developers: developer ? [developer] : [], releaseYear: year, imagePath: image, genres, mechanics, about: description };
            const res = await apiFetch(`${API_BASE}/${editId}?performedBy=${encodeURIComponent(username)}`, { method: 'PUT', headers: {'Content-Type':'application/json'}, body: JSON.stringify(updated) });
            if (res.ok) {
                closeModal('adminModal');
                const games = await fetchAllGames();
                if (games) { appState.allGames = games; appState.games = games; }
                renderAdminTable(appState.allGames);
                alert('Igra uspješno ažurirana.');
            } else {
                const text = await res.text();
                console.error('Update error:', text);
                alert('Greška pri ažuriranju igre: ' + (text || res.statusText));
            }
        }
    });

    // Delete confirm
    document.getElementById('confirmDeleteBtn')?.addEventListener('click', async () => {
        if (!deletePendingId) return;
        const username = appState.currentUser || 'luka';
        const ok = await deleteGameAPI(deletePendingId, username);
        if (ok) {
            document.getElementById('confirmDeleteModal')?.classList.add('hidden');
            deletePendingId = null;
            const games = await fetchAllGames();
            if (games) { appState.allGames = games; appState.games = games; }
            renderAdminTable(appState.allGames);
        } else alert('Greška pri brisanju igre.');
    });

    // Zatvaranje modala (generalno za game/admin modale)
    document.querySelectorAll('.close-modal, .modal-overlay').forEach(el => {
        el.addEventListener('click', () => {
            closeModal('gameModal');
            closeModal('adminModal');
        });
    });

    // Close admin / confirm modals and cancel buttons
    document.querySelectorAll('.close-admin-modal, .close-confirm-modal').forEach(el => el.addEventListener('click', () => {
        closeModal('adminModal');
        closeModal('confirmDeleteModal');
    }));

    // Admin: Dugme za dodavanje
    document.getElementById('addNewGameBtn')?.addEventListener('click', () => {
        openAdminFormModal('create');
    });
}