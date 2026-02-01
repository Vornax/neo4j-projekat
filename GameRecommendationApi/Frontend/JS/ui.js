/**
 * ui.js
 * Upravljanje vidljivošću elemenata, modalima i navigacijom
 */

import { appState } from './state.js';


// Menja prikaz između User i Admin interfejsa.
export function toggleUserAdminView(role) {
    const userMain = document.getElementById('userMain');
    const adminMain = document.getElementById('adminMain');
    const mainNav = document.querySelector('.main-nav');
    const isAdmin = role === 'admin';

    if (isAdmin) {
        userMain?.classList.add('hidden');
        adminMain?.classList.remove('hidden');
        // Sakrij glavnu navigaciju dok je Admin prikaz
        mainNav?.classList.add('hidden');
    } else {
        userMain?.classList.remove('hidden');
        adminMain?.classList.add('hidden');
        mainNav?.classList.remove('hidden');
    }
}


// Upravlja promenom tabova (Store, Wishlist, Recommendations)
export function switchTabUI(tab) {
    const resultsTitle = document.getElementById('resultsTitle');
    const recSection = document.getElementById('recommendationsSection');
    const resultsSection = document.querySelector('.results-section');
    const searchFilterSection = document.querySelector('.search-filter-section');

    // Resetuj sve sekcije
    recSection?.classList.add('hidden');
    resultsSection?.classList.remove('hidden');
    searchFilterSection?.classList.remove('hidden');

    if (tab === 'recommendations') {
        resultsSection?.classList.add('hidden');
        searchFilterSection?.classList.add('hidden');
        recSection?.classList.remove('hidden');
    } else if (tab === 'wishlist') {
        if (resultsTitle) resultsTitle.innerText = 'Moja Lista Želja';
        searchFilterSection?.classList.add('hidden');
    } else {
        if (resultsTitle) resultsTitle.innerText = 'Sve Igre';
    }
}


// Otvara modal sa detaljima igre
export function openDetailsModal(game) {
    const modal = document.getElementById('gameModal');
    if (!modal) return;

    document.getElementById('modalTitle').innerText = game.title;
    document.getElementById('modalImg').src = game.image;

    const studioEl = document.getElementById('modalStudio');
    if (studioEl) studioEl.querySelector('span').innerText = game.developer || (game.developers && game.developers[0]) || 'Unknown';
    const yearEl = document.getElementById('modalYear');
    if (yearEl) yearEl.querySelector('span').innerText = game.year || game.releaseYear || '';

    const aboutEl = document.getElementById('modalAbout');
    if (aboutEl) aboutEl.innerText = game.about || game.description || 'Nema opisa.';

    const genreTags = document.getElementById('modalGenres');
    if (genreTags) genreTags.innerHTML = (game.genres || []).map(g => `<span class="tag">${g}</span>`).join('');
    
    const mechTags = document.getElementById('modalMechanics');
    if (mechTags) mechTags.innerHTML = (game.mechanics || []).map(m => `<span class="tag secondary">${m}</span>`).join('');

    modal.classList.remove('hidden');
}

// Zatvara bilo koji modal koji ima klasu 'hidden'
export function closeModal(modalId) {
    document.getElementById(modalId)?.classList.add('hidden');
}

// Otvara Admin modal za dodavanje ili izmenu igre
export function openAdminFormModal(mode, game = null) {
    const modal = document.getElementById('adminModal');
    const title = document.getElementById('adminModalTitle');
    const form = document.getElementById('adminGameForm');
    
    if (!modal || !form) return;

    title.innerText = mode === 'edit' ? 'Izmeni Igru' : 'Dodaj Novu Igru';
    form.dataset.mode = mode;
    form.dataset.editId = game ? game.id : '';

    // Popuni polja ako je edit 
    const idEl = document.getElementById('adminGameId');
    const titleEl = document.getElementById('adminTitle');
    const devEl = document.getElementById('adminDeveloper');
    const yearEl = document.getElementById('adminYear');
    const imgEl = document.getElementById('adminImage');
    const genresEl = document.getElementById('adminGenres');
    const mechsEl = document.getElementById('adminMechanics');
    const descEl = document.getElementById('adminDescription');

    if (idEl) idEl.value = game ? (game.id || '') : '';
    if (titleEl) titleEl.value = game ? (game.title || '') : '';
    if (devEl) devEl.value = game ? (game.developer || (game.developers ? (Array.isArray(game.developers) ? game.developers.join(', ') : game.developers) : '') ) : '';    if (yearEl) yearEl.value = game ? (game.year || game.releaseYear || '') : '';
    if (imgEl) {
        if (!game) imgEl.value = '';
        else {
            const raw = game.imagePath || game.image || '';

            if (raw && raw.startsWith('/')) imgEl.value = window.location.origin + raw;
            else imgEl.value = raw;
        }
    }
    if (genresEl) genresEl.value = game ? (game.genres ? game.genres.join(', ') : '') : '';
    if (mechsEl) mechsEl.value = game ? (game.mechanics ? game.mechanics.join(', ') : '') : '';
    if (descEl) descEl.value = game ? (game.description || '') : '';

    modal.classList.remove('hidden');
}

