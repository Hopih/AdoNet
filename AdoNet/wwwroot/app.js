async function api(method, url, body) {
    const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: body ? JSON.stringify(body) : undefined
    });
    const text = await response.text();
    if (!response.ok) throw new Error(text || response.statusText);
    return text ? JSON.parse(text) : null;
}

async function run(method, url, body) {
    try {
        await api(method, url, body);
        await load();
        return true;
    } catch (e) {
        alert(e.message);
        return false;
    }
}

function button(text, onClick) {
    const b = document.createElement('button');
    b.textContent = text;
    b.onclick = onClick;
    return b;
}

// columns: { 'Заголовок': row => значение }
function renderTable(id, rows, columns, onEdit, onDelete) {
    const table = document.getElementById(id);
    table.innerHTML = '';

    const head = table.insertRow();
    for (const title of [...Object.keys(columns), '']) head.insertCell().textContent = title;

    for (const row of rows) {
        const tr = table.insertRow();
        for (const getValue of Object.values(columns)) tr.insertCell().textContent = getValue(row);
        tr.insertCell().append(button('Изменить', () => onEdit(row)), button('Удалить', () => onDelete(row)));
    }
}

function fillSelect(id, rows, getText) {
    const select = document.getElementById(id);
    select.innerHTML = '';
    for (const row of rows) select.add(new Option(getText(row), row.id));
}

const remove = url => row => run('DELETE', url + row.id);

async function load() {
    const [users, projects, stages] = await Promise.all([
        api('GET', '/api/users'),
        api('GET', '/api/projects'),
        api('GET', '/api/stages')
    ]);

    renderTable('users', users, {
        'Имя': u => u.name,
        'Email': u => u.email
    }, editUser, remove('/api/users/'));

    renderTable('projects', projects, {
        'Адрес': p => p.address,
        'Владелец': p => p.userName,
        'Начало': p => p.startDate,
        'Бюджет': p => p.budget,
        'Потрачено (функция)': p => p.totalCost
    }, editProject, remove('/api/projects/'));

    renderTable('stages', stages, {
        'Проект': s => s.projectAddress,
        'Название': s => s.title,
        'Стоимость': s => s.cost,
        'Статус': s => s.status
    }, editStage, remove('/api/stages/'));

    fillSelect('project-user', users, u => u.name);
    fillSelect('stage-project', projects, p => p.address);
}

function editUser(u) {
    const name = prompt('Имя', u.name);
    const email = prompt('Email', u.email);
    if (name && email) run('PUT', '/api/users/' + u.id, { ...u, name, email });
}

function editProject(p) {
    const address = prompt('Адрес', p.address);
    const budget = prompt('Бюджет', p.budget);
    if (address && budget) run('PUT', '/api/projects/' + p.id, { ...p, address, budget });
}

function editStage(s) {
    const title = prompt('Название', s.title);
    const cost = prompt('Стоимость', s.cost);
    const status = prompt('Статус (Запланирован / В работе / Завершён)', s.status);
    if (title && cost && status) run('PUT', '/api/stages/' + s.id, { ...s, title, cost, status });
}

for (const [formId, url] of [
    ['register-form', '/api/users'],
    ['project-form', '/api/projects'],
    ['stage-form', '/api/stages']
]) {
    const form = document.getElementById(formId);
    form.onsubmit = async e => {
        e.preventDefault();
        const data = Object.fromEntries(new FormData(form));
        if (await run('POST', url, data)) form.reset();
    };
}

load();
