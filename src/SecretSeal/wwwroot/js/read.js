import "./version.js?v=20261030";

import {
    NOT_FOUND,
    mustGet,
    setResult,
    readJsonSafe,
    b64urlToBytes,
    importAesKeyFromB64url,
} from "./shared.js?v=20261030";

// =============================
// Constants / DOM
// =============================
const openBtn = mustGet("openBtn");
const resultEl = mustGet("result");

// A normal #main link would replace the decryption key in the URL.
document.querySelector(".skip-link").addEventListener("click", (event) => {
    event.preventDefault();
    mustGet("main").focus();
    mustGet("main").scrollIntoView();
});

function setResultEl(kind, text) {
    setResult(resultEl, kind, text);
}

// =============================
// URL / key
// =============================
const id = (() => {
    const v = (new URLSearchParams(window.location.search).get("id") || "").trim();
    return v || null;
})();

function getKeyFromHash() {
    const raw = (window.location.hash || "").replace(/^#/, "").trim();
    if (!raw) return null;
    const hp = new URLSearchParams(raw);
    const k = (hp.get("k") || "").trim();
    return k || null;
}

// =============================
// UI helpers
// =============================
function disableForever(btn) {
    btn.disabled = true;
    btn.dataset.used = "1";
}

// =============================
// Fetch helpers
// =============================
async function apiConsumeNote(noteId) {
    const res = await fetch(`/notes/${encodeURIComponent(noteId)}`, { method: "DELETE" });
    const data = await readJsonSafe(res);

    if (!res.ok) {
        const msg = (data && data.error) ? String(data.error) : NOT_FOUND;
        throw new Error(msg || NOT_FOUND);
    }

    return data;
}

// =============================
// Crypto helpers (AES-256-GCM)
// =============================
async function decryptFromPayloadString(payloadString, keyB64url) {
    let payload;
    try { payload = JSON.parse(payloadString); }
    catch { throw new Error(NOT_FOUND); }

    if (!payload || payload.v !== 1 || payload.alg !== "A256GCM" || !payload.iv || !payload.ct) {
        throw new Error(NOT_FOUND);
    }

    let iv, ct;
    try {
        iv = b64urlToBytes(payload.iv);
        ct = b64urlToBytes(payload.ct);
    } catch {
        throw new Error(NOT_FOUND);
    }

    const key = await importAesKeyFromB64url(keyB64url, ["decrypt"]);

    try {
        const pt = new Uint8Array(await crypto.subtle.decrypt({ name: "AES-GCM", iv }, key, ct));
        return new TextDecoder().decode(pt);
    } catch {
        throw new Error(NOT_FOUND);
    }
}

// =============================
// One-shot action (hard-guarded)
// =============================
let inFlight = false;

async function openSecretOnce() {
    // HARD GUARD (covers double-click, keyboard, re-entrancy)
    if (inFlight || openBtn.dataset.used === "1" || openBtn.disabled) return;
    inFlight = true;

    // Disable immediately and remove handler to make it truly one-shot
    disableForever(openBtn);
    openBtn.setAttribute("aria-busy", "true");
    openBtn.textContent = "Opening…";
    openBtn.removeEventListener("click", openSecretOnce);

    setResultEl(null, "");

    try {
        if (!id) throw new Error("No secret ID provided in URL.");

        const key = getKeyFromHash();
        if (!key) throw new Error(NOT_FOUND);

        const data = await apiConsumeNote(id);

        const payloadString = typeof data?.note === "string" ? data.note : "";
        if (!payloadString) throw new Error(NOT_FOUND);

        const plaintext = await decryptFromPayloadString(payloadString, key);
        setResultEl("success", `Secret:\n\n${plaintext}`);
        mustGet("open-heading").textContent = "For your eyes only.";
        mustGet("openDescription").textContent = "This secret has been deleted from the server.";
        mustGet("openHint").textContent = "Keep what you need before leaving this page. It can't be opened again.";
        openBtn.textContent = "Secret opened";
    } catch (err) {
        setResultEl("error", err?.message || NOT_FOUND);
        mustGet("open-heading").textContent = "Secret unavailable.";
        mustGet("openDescription").textContent = "This link may be incomplete, expired or already opened.";
        mustGet("openHint").textContent = "Ask the sender for a new secret link.";
        openBtn.textContent = "Unable to open";
    } finally {
        openBtn.setAttribute("aria-busy", "false");
    }
}

// =============================
// Init and fragment changes
// =============================
function updateLinkState() {
    if (openBtn.dataset.used === "1") return;

    if (!id || !getKeyFromHash()) {
        openBtn.disabled = true;
        openBtn.textContent = "Link required";
        mustGet("open-heading").textContent = id ? "A link is incomplete." : "A link is missing.";
        mustGet("openDescription").textContent = "Use the complete secret link shared by the sender.";
        mustGet("openHint").textContent = "Ask the sender for their secret link.";
        setResultEl("error", id ? "No decryption key provided in URL." : "No secret ID provided in URL.");
        return;
    }

    openBtn.disabled = false;
    openBtn.textContent = "Open & destroy";
    mustGet("open-heading").textContent = "A secret awaits.";
    mustGet("openDescription").textContent = "One chance to read it. Once opened, it's deleted for good.";
    mustGet("openHint").textContent = "Your secret will appear here. Only open it when you're ready.";
    setResultEl(null, "");
    openBtn.addEventListener("click", openSecretOnce, { passive: true });
}

updateLinkState();
window.addEventListener("hashchange", updateLinkState);
