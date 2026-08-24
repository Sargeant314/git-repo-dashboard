const API_URL = '/api/projects';

//Get request for all projects
async function fetchProjects() {
    try {
        const response = await fetch(API_URL);
        const projects = await response.json();
        renderProjects(projects);
    } catch (err) {
        console.error('Error fetching projects:', err);
    }
}

//Generate a card for each project and append to project grid
function renderProjects(projects) {
    const grid = document.getElementById('project-grid');
    if (projects.length === 0) {
        grid.innerHTML = `<div class="col-span-full text-center py-12 text-gray-500">No project notes found. Click "Add Project Note" to get started!</div>`;
        return;
    }

    grid.innerHTML = projects.map(p => `
        <div class="bg-gray-800 border border-gray-700/60 rounded-xl p-5 shadow-lg flex flex-col justify-between hover:border-gray-600 transition">
            <div>
                <div class="flex justify-between items-start mb-3">
                    <h3 class="text-lg font-bold text-white break-all">${escapeHtml(p.repoName)}</h3>
                    <span class="px-2.5 py-1 text-xs font-semibold rounded-full ${getPriorityBadge(p.priorityLevel)}">${escapeHtml(p.priorityLevel)}</span>
                </div>
                <p class="text-sm text-gray-300 mb-4 bg-gray-900/50 p-3 rounded-lg border border-gray-800/80">${escapeHtml(p.privateNotes || 'No notes provided.')}</p>
            </div>
            <div class="flex items-center justify-between pt-4 border-t border-gray-700/50">
                <span class="text-xs text-gray-400 flex items-center gap-1.5"><span class="w-2 h-2 rounded-full ${getStatusDot(p.status)}"></span>${escapeHtml(p.status)}</span>
                <div class="flex gap-2">
                    <button onclick="editProject(${p.id}, '${escapeQuote(p.repoName)}', '${p.priorityLevel}', '${p.status}', '${escapeQuote(p.privateNotes)}')" class="text-xs text-indigo-400 hover:text-indigo-300 font-medium cursor-pointer">Edit</button>
                    <button onclick="deleteProject(${p.id})" class="text-xs text-red-400 hover:text-red-300 font-medium cursor-pointer">Delete</button>
                </div>
            </div>
        </div>
    `).join('');
}

//Helper to change style based on projects priority
function getPriorityBadge(priority) {
    if (priority === 'High') return 'bg-red-500/10 text-red-400 border border-red-500/20';
    if (priority === 'Medium') return 'bg-amber-500/10 text-amber-400 border border-amber-500/20';
    return 'bg-blue-500/10 text-blue-400 border border-blue-500/20';
}

//Helper to change style based on projects status
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

function closeModal() {
    document.getElementById('project-modal').classList.add('hidden');
}

function editProject(id, repoName, priorityLevel, status, privateNotes) {
    document.getElementById('project-id').value = id;
    document.getElementById('repoName').value = repoName;
    document.getElementById('priorityLevel').value = priorityLevel;
    document.getElementById('status').value = status;
    document.getElementById('privateNotes').value = privateNotes;
    document.getElementById('modal-title').innerText = 'Edit Project Note';
    document.getElementById('project-modal').classList.remove('hidden');
}

//listener to submit form data to backend
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
        const response = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(projectData)
        });

        if (response.ok || response.status === 204) {
            closeModal();
            fetchProjects();
        } else {
            alert('Failed to save project.');
        }
    } catch (err) {
        console.error('Error saving project:', err);
    }
}


async function deleteProject(id) {
    if (!confirm('Are you sure you want to delete this project note?')) return;
    try {
        const response = await fetch(`${API_URL}/${id}`, { method: 'DELETE' });
        if (response.ok || response.status === 204) {
            fetchProjects();
        } else {
            alert('Failed to delete project.');
        }
    } catch (err) {
        console.error('Error deleting project:', err);
    }
}

function escapeHtml(str) {
    return (str || '').replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
}

function escapeQuote(str) {
    return (str || '').replace(/'/g, "\\'");
}

// Initial fetch on page load
fetchProjects();