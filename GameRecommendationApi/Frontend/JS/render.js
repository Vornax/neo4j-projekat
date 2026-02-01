/**
 * render.js
 * Generisanje HTML struktura i prikazivanje podataka na korisničkom interfejsu
 */

import { appState } from './state.js';


// Renderuje listu igara u specifičan kontejner (podrazumevano 'gamesGrid').
export function renderGames(list, containerId = 'gamesGrid') {
    const container = document.getElementById(containerId);
    if (!container) return;

    container.innerHTML = '';
    
    if (!list || !list.length) {
        container.innerHTML = '<p class="muted">Nema rezultata</p>';
        return;
    }

    list.forEach(g => {
        const liked = appState.favorites.includes(Number(g.id)) ? 'liked' : '';
        const img = g.image || 'https://via.placeholder.com/220x300?text=No+Image';
        const dev = (g.developer || (g.developers && g.developers[0]) || 'Unknown');
        const year = g.releaseYear || g.year || '';

        container.insertAdjacentHTML('beforeend', `
            <div class="game-card" data-id="${g.id}">
                <div class="card-img"><img src="${img}" alt="${g.title}"></div>
                <div class="card-body">
                    <h3 class="game-title">${g.title}</h3>
                    <p class="game-meta">${dev} · ${year}</p>
                </div>
                <button class="fav-btn ${liked}" data-id="${g.id}" aria-label="Dodaj u listu">♥</button>
            </div>
        `);
    });
}


// Renderuje checkbox filtere na osnovu podataka sa servera ili mock podataka
export function renderFilters(filterData) {
    const { genres, mechanics, developers } = filterData;
    
    const genreContainer = document.getElementById('genreFilters');
    const mechanicContainer = document.getElementById('mechanicFilters');
    const devContainer = document.getElementById('developerFilters');

    if (genreContainer) {
        genreContainer.innerHTML = genres?.length 
            ? genres.sort().map(g => `<label class="filter-tag"><input type="checkbox" value="${g}" class="filter-checkbox" data-type="genre"> ${g}</label>`).join('')
            : '<p class="muted">Nema žanrova</p>';
    }

    if (mechanicContainer) {
        mechanicContainer.innerHTML = mechanics?.length 
            ? mechanics.sort().map(m => `<label class="filter-tag"><input type="checkbox" value="${m}" class="filter-checkbox" data-type="mechanic"> ${m}</label>`).join('')
            : '<p class="muted">Nema mehanika</p>';
    }

    if (devContainer) {
        devContainer.innerHTML = developers?.length 
            ? developers.sort().map(d => `<label class="filter-tag"><input type="checkbox" value="${d}" class="filter-checkbox" data-type="developer"> ${d}</label>`).join('')
            : '<p class="muted">Nema developera</p>';
    }
}


// Popunjava tabelu u Admin panelu
export function renderAdminTable(games) {
    const tbody = document.getElementById('adminTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';
    games.forEach(g => {
        const row = document.createElement('tr');
        const dev = (g.developer || (g.developers && g.developers[0]) || 'Unknown');
        row.innerHTML = `
            <td><img class="admin-thumb" src="${g.image}" alt="${g.title}" /></td>
            <td>${g.title}</td>
            <td>${dev}</td>
            <td>${g.year || g.releaseYear || ''}</td>
            <td>
              <button class="edit-game-btn" data-id="${g.id}">Edit</button>
              <button class="delete-game-btn" data-id="${g.id}">Delete</button>
            </td>`;
        tbody.appendChild(row);
    });
}


// Popunjava padajući meni za izbor korisnika
export function renderUserSelect(users, currentUserId) {
    const select = document.getElementById('userMode');
    if (!select) return;

    select.innerHTML = '';
    if (!users || !users.length) {
        select.insertAdjacentHTML('beforeend', `<option value="guest">Korisnik (guest)</option>`);
        select.insertAdjacentHTML('beforeend', `<option value="admin">Admin (admin)</option>`);
    } else {
        users.forEach(u => {
            const opt = document.createElement('option');
            opt.value = u.username;
            opt.innerText = u.role === 'admin' ? `Admin (${u.username})` : `Korisnik (${u.username})`;
            if (u.username === currentUserId) opt.selected = true;
            select.appendChild(opt);
        });
    }
}

