export const NOT_FOUND = "Note not found (or already consumed).";

export function mustGet(id) {
    const el = document.getElementById(id);
    if (!el) throw new Error(`Missing element #${id}`);
    return el;
}

export function setResult(el, kind, text) {
    el.textContent = text || "";
    el.className = "result";
    if (kind) el.classList.add(kind);
}

export function setBusy(btn, isBusy) {
    btn.disabled = isBusy;
    btn.setAttribute("aria-busy", String(isBusy));
}

export async function readJsonSafe(response) {
    const text = await response.text();
    if (!text) return null;
    try { return JSON.parse(text); }
    catch { return { error: text }; }
}

export function bytesToB64url(bytes) {
    const bin = String.fromCharCode(...bytes);
    return btoa(bin).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "");
}

export function b64urlToBytes(b64url) {
    const b64 = b64url.replace(/-/g, "+").replace(/_/g, "/") + "===".slice((b64url.length + 3) % 4);
    const bin = atob(b64);
    const bytes = new Uint8Array(bin.length);
    for (let i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
    return bytes;
}

export const utf8 = {
    toBytes: (s) => new TextEncoder().encode(s),
    fromBytes: (b) => new TextDecoder().decode(b),
};

export function randBytes(n) {
    const a = new Uint8Array(n);
    crypto.getRandomValues(a);
    return a;
}

export async function importAesKeyFromB64url(keyB64url, usages = ["decrypt"]) {
    const raw = b64urlToBytes(keyB64url);
    if (raw.length !== 32) throw new Error(NOT_FOUND);
    return crypto.subtle.importKey("raw", raw, { name: "AES-GCM" }, false, usages);
}
