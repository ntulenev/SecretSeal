import {
    NOT_FOUND,
    mustGet,
    setResult,
    readJsonSafe,
    b64urlToBytes,
    importAesKeyFromB64url,
} from "./shared.js";

// =============================
// Constants / DOM
// =============================
const openBtn = mustGet("openBtn");
const resultEl = mustGet("result");

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
    btn.setAttribute("aria-busy", "true");
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
    } catch (err) {
        setResultEl("error", err?.message || NOT_FOUND);
        // keep disabled forever (old behavior you want)
    }
}

// =============================
// Init
// =============================
if (!id) {
    disableForever(openBtn);
    setResultEl("error", "No secret ID provided in URL.");
} else {
    openBtn.addEventListener("click", openSecretOnce, { passive: true });

    // Optional: Enter triggers open once as well
    window.addEventListener("keydown", (e) => {
        if (e.key !== "Enter") return;
        openSecretOnce();
    });
}
