const API_URL = '/api/projects';
let isLoginMode = true;

// --- CORE API FETCHER (Handles Authentication Headers automatically) ---
async function apiFetch(url, options = {}) {
    const token = localStorage.getItem('token');
    
    if (!options.headers) options.headers = {};
    if (token) options.headers['Authorization'] = `Bearer ${token}`;

    const response = await fetch(url, options);

    // If the API says "Unauthorized", our token is invalid/expired. Log the user out.
    if (response.status === 401) {
        logout();
        throw new Error("Session expired. Please log in again.");
    }
    return response;
}

// --- AUTHENTICATION UI & LOGIC ---
function checkAuthState() {
    const token = localStorage.getItem('token');
    if (token) {
        document.getElementById('auth-section').classList.add('hidden');
        document.getElementById('dashboard-section').classList.remove('hidden');
        fetchProjects(); // Load user data
    } else {
        document.getElementById('auth-section').classList.remove('hidden');
        document.getElementById('dashboard-section').classList.add('hidden');
    }
}

function toggleAuthMode() {
    isLoginMode = !isLoginMode;
    document.getElementById('auth-title').innerText = isLoginMode ? 'Log In to Dashboard' : 'Create an Account';
    document.getElementById('auth-button').innerText = isLoginMode ? 'Log In' : 'Register';
    document.getElementById('auth-toggle-text').innerText = isLoginMode ? "Don't have an account?" : "Already have an account?";
    document.getElementById('auth-toggle-link').innerText = isLoginMode ? 'Register' : 'Log In';
}

async function handleAuth(event) {
            event.preventDefault();
            const email = document.getElementById('auth-email').value;
            const password = document.getElementById('auth-password').value;
            
            const endpoint = isLoginMode ? '/login' : '/register';
            
            try {
                const response = await fetch(endpoint, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email, password })
                });

                if (response.ok) {
                    if (isLoginMode) {
                        const data = await response.json();
                        // ASP.NET Core Identity returns accessToken upon successful login
                        if (data.accessToken) {
                            localStorage.setItem('token', data.accessToken);
                            document.getElementById('auth-email').value = '';
                            document.getElementById('auth-password').value = '';
                            checkAuthState();
                        } else {
                            alert('Login succeeded, but token was missing.');
                        }
                    } else {
                        alert('Registration successful! Please log in.');
                        toggleAuthMode(); // Switch back to the login view
                    }
                } else {
                    const error = await response.json();
                    let errorMessage = error.title || 'Invalid credentials or request';
                    
                    if (error.errors) {
                        const detailedErrors = Object.values(error.errors).flat().join('\n• ');
                        errorMessage = `Please fix the following:\n• ${detailedErrors}`;
                    } else if (error.detail) {
                        errorMessage = error.detail;
                    }
                    
                    alert(errorMessage);
                }
            } catch (err) {
                console.error('Auth error:', err);
                alert('Could not connect to the authentication server.');
            }
        }

function logout() {
    localStorage.removeItem('token');
    checkAuthState();
}

// --- DASHBOARD LOGIC (Updated to use apiFetch) ---
async function fetchProjects() {
    try {
        const response = await apiFetch(API_URL);
        const projects = await response.json();
        renderProjects(projects);
    } catch (err) {
        console.error(err);
    }
}

async function importSingleRepo() {
    const input = document.getElementById('github-input').value.trim();
    if (!input || !input.includes('/')) {
        alert('For a single repo, please use the format: username/reponame'); return;
    }
    const [username, repoName] = input.split('/');
    try {
        const response = await apiFetch(`/api/projects/import-github/${username}/${repoName}`, { method: 'POST' });
        if (response.ok) {
            alert(`Successfully imported ${input}!`);
            document.getElementById('github-input').value = '';
            fetchProjects();
        } else if (response.status === 409) alert('This repository is already in your dataset.');
        else if (response.status === 404) alert('Repository not found on GitHub.');
        else alert('Failed to import.');
    } catch (err) {}
}

async function importAllRepos() {
    const input = document.getElementById('github-input').value.trim();
    if (!input || input.includes('/')) {
        alert('For bulk import, just enter the username (no slashes).'); return;
    }
    try {
        const response = await apiFetch(`/api/projects/import-github/${input}`, { method: 'POST' });
        if (response.ok) {
            const data = await response.json();
            alert(`Successfully imported ${data.importedCount} new repositories!`);
            document.getElementById('github-input').value = '';
            fetchProjects();
        } else alert('Failed to fetch repositories.');
    } catch (err) {}
}

async function saveProject(event) {
    event.preventDefault();
    const id = document.getElementById('project-id').value;
    const projectData = {
        id: id ? parseInt(id) : 0,
        repoName: document.getElementById('repoName').value,
        priorityLevel: document.getElementById('priorityLevel').value,
        status: document.getElementById('status').value,
        privateNotes: document.getElementById('privateNotes').value
    };

    const method = id ? 'PUT' : 'POST';
    const url = id ? `${API_URL}/${id}` : API_URL;

    try {
        const response = await apiFetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(projectData)
        });
        if (response.ok || response.status === 204) {
            closeModal();
            fetchProjects();
        }
    } catch (err) {}
}

async function deleteProject(id) {
    if (!confirm('Are you sure you want to delete this project note?')) return;
    try {
        const response = await apiFetch(`${API_URL}/${id}`, { method: 'DELETE' });
        if (response.ok || response.status === 204) fetchProjects();
    } catch (err) {}
}

function renderProjects(projects) {
    const grid = document.getElementById('project-grid');
    if (projects.length === 0) {
        grid.innerHTML = `<div class="col-span-full text-center py-12 text-gray-500">No project notes found. Click "+ Add Note" or import from GitHub to get started!</div>`;
        return;
    }
    grid.innerHTML = projects.map(p => `
        <div class="bg-gray-800 border border-gray-700/60 rounded-xl p-5 shadow-lg flex flex-col justify-between hover:border-gray-600 transition">
            <div>
                <div class="flex justify-between items-start mb-3 gap-2">
                    <h3 class="text-lg font-bold text-white break-all flex-1">${escapeHtml(p.repoName)}</h3>
                    <div class="flex gap-2 shrink-0">
                        <span class="px-2.5 py-1 text-xs font-semibold rounded-full bg-gray-900 text-gray-300 border border-gray-600">${escapeHtml(p.language || 'Code')}</span>
                        <span class="px-2.5 py-1 text-xs font-semibold rounded-full ${getPriorityBadge(p.priorityLevel)}">${escapeHtml(p.priorityLevel)}</span>
                    </div>
                </div>
                <p class="text-sm text-gray-300 mb-4 bg-gray-900/50 p-3 rounded-lg border border-gray-800/80 whitespace-pre-wrap">${escapeHtml(p.privateNotes || 'No notes provided.')}</p>
            </div>
            <div class="flex items-center justify-between pt-4 border-t border-gray-700/50">
                <span class="text-xs text-gray-400 flex items-center gap-1.5"><span class="w-2 h-2 rounded-full ${getStatusDot(p.status)}"></span>${escapeHtml(p.status)}</span>
                <div class="flex gap-3">
                    <button onclick="editProject(${p.id}, '${escapeQuote(p.repoName)}', '${p.priorityLevel}', '${p.status}', '${escapeQuote(p.privateNotes)}')" class="text-xs text-indigo-400 hover:text-indigo-300 font-medium">Edit</button>
                    <button onclick="deleteProject(${p.id})" class="text-xs text-red-400 hover:text-red-300 font-medium">Delete</button>
                </div>
            </div>
        </div>
    `).join('');
}

// --- HELPER FUNCTIONS ---
function getPriorityBadge(priority) {
    if (priority === 'High') return 'bg-red-500/10 text-red-400 border border-red-500/20';
    if (priority === 'Medium') return 'bg-amber-500/10 text-amber-400 border border-amber-500/20';
    return 'bg-blue-500/10 text-blue-400 border border-blue-500/20';
}
function getStatusDot(status) {
    if (status === 'Completed') return 'bg-emerald-400';
    if (status === 'In Progress') return 'bg-indigo-400';
    return 'bg-gray-500';
}
function openModal() {
    document.getElementById('project-form').reset();
    document.getElementById('project-id').value = '';
    document.getElementById('modal-title').innerText = 'Add New Project Note';
    document.getElementById('project-modal').classList.remove('hidden');
}
function closeModal() { document.getElementById('project-modal').classList.add('hidden'); }
function editProject(id, repoName, priorityLevel, status, privateNotes) {
    document.getElementById('project-id').value = id;
    document.getElementById('repoName').value = repoName;
    document.getElementById('priorityLevel').value = priorityLevel;
    document.getElementById('status').value = status;
    document.getElementById('privateNotes').value = privateNotes;
    document.getElementById('modal-title').innerText = 'Edit Project Note';
    document.getElementById('project-modal').classList.remove('hidden');
}
function escapeHtml(str) { return (str || '').replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;"); }
function escapeQuote(str) { return (str || '').replace(/'/g, "\\'"); }

// Start App
checkAuthState();