const { app, BrowserWindow, dialog, net, protocol, shell } = require('electron');
const { stat } = require('node:fs/promises');
const path = require('node:path');
const { pathToFileURL } = require('node:url');

const appUrl = 'gwire://app/';
const contentTypes = {
    '.html': 'text/html; charset=utf-8',
    '.js': 'text/javascript; charset=utf-8',
    '.mjs': 'text/javascript; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
    '.json': 'application/json',
    '.webmanifest': 'application/manifest+json',
    '.wasm': 'application/wasm',
    '.svg': 'image/svg+xml'
};

protocol.registerSchemesAsPrivileged([{
    scheme: 'gwire',
    privileges: { standard: true, secure: true, supportFetchAPI: true, corsEnabled: true }
}]);

function isAppUrl(value) {
    const url = new URL(value);
    return url.protocol === 'gwire:' && url.host === 'app'
        && !url.username && !url.password;
}

async function serveClient(request) {
    if (!isAppUrl(request.url)) {
        return new Response(null, { status: 403 });
    }
    if (request.method !== 'GET' && request.method !== 'HEAD') {
        return new Response(null, { status: 405 });
    }

    const root = path.join(process.resourcesPath, 'client');
    const pathname = decodeURIComponent(new URL(request.url).pathname);
    const filePath = path.resolve(root, `.${pathname}`);
    const relativePath = path.relative(root, filePath);
    if (relativePath.startsWith('..') || path.isAbsolute(relativePath)) {
        return new Response(null, { status: 403 });
    }

    let target = filePath;
    try {
        if (!(await stat(target)).isFile()) {
            target = path.join(root, 'index.html');
        }
    } catch (error) {
        if (error.code !== 'ENOENT' && error.code !== 'ENOTDIR') {
            throw error;
        }
        // Only document navigation falls back to the Blazor router.
        if (request.mode !== 'navigate') {
            return new Response(null, { status: 404 });
        }
        target = path.join(root, 'index.html');
    }

    const response = await net.fetch(pathToFileURL(target).href);
    const headers = new Headers(response.headers);
    const contentType = contentTypes[path.extname(target).toLowerCase()];
    if (contentType) {
        headers.set('Content-Type', contentType);
    }
    return new Response(request.method === 'HEAD' ? null : response.body, {
        status: response.status, headers
    });
}

async function openExternal(url) {
    if (new URL(url).protocol === 'https:') {
        await shell.openExternal(url);
    }
}

async function createWindow() {
    const window = new BrowserWindow({
        width: 1440,
        height: 960,
        webPreferences: { nodeIntegration: false, contextIsolation: true, sandbox: true }
    });

    window.webContents.setWindowOpenHandler(({ url }) => {
        openExternal(url).catch(error => dialog.showErrorBox('Unable to open link', error.message));
        return { action: 'deny' };
    });
    window.webContents.on('will-navigate', (event, url) => {
        if (!isAppUrl(url)) {
            event.preventDefault();
            openExternal(url).catch(error => dialog.showErrorBox('Unable to open link', error.message));
        }
    });
    await window.loadURL(appUrl);
}

async function start() {
    await app.whenReady();
    protocol.handle('gwire', serveClient);
    await createWindow();
    app.on('activate', async () => {
        if (BrowserWindow.getAllWindows().length === 0) {
            try {
                await createWindow();
            } catch (error) {
                dialog.showErrorBox('Unable to open Gwire', error.message);
            }
        }
    });
}

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

start().catch(error => {
    dialog.showErrorBox('Unable to start Gwire', error.message);
    app.quit();
});
