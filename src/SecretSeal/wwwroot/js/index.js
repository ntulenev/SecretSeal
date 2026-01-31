import {
    NOT_FOUND,
    mustGet,
    setResult,
    setBusy,
    readJsonSafe,
    bytesToB64url,
    b64urlToBytes,
    utf8,
    randBytes,
    importAesKeyFromB64url,
} from "./shared.js";

// =============================
// Constants
// =============================
const API = {
    create: "/notes",
    consume: (id) => `/notes/${encodeURIComponent(id)}`,
    retention: "/retention-policy",
};

// =============================
// DOM
// =============================
const els = {
    createNote: mustGet("createNote"),
    createBtn: mustGet("createBtn"),
    createResult: mustGet("createResult"),
    createShare: mustGet("createShare"),
    guidBox: mustGet("guidBox"),
    linkBox: mustGet("linkBox"),
    copyIdBtn: mustGet("copyIdBtn"),
    copyLinkBtn: mustGet("copyLinkBtn"),
    retentionNote: mustGet("retentionNote"),

    readId: mustGet("readId"),
    readBtn: mustGet("readBtn"),
    readResult: mustGet("readResult"),
};

// =============================
// UI helpers
// =============================
function showShare({ compositeId, link }) {
    els.guidBox.textContent = compositeId;
    els.linkBox.textContent = link;
    els.createShare.style.display = "block";
}

function hideShare() {
    els.createShare.style.display = "none";
    els.guidBox.textContent = "";
    els.linkBox.textContent = "";
    els.retentionNote.style.display = "none";
    els.retentionNote.textContent = "";
}

// =============================
// Fetch helpers
// =============================
async function apiJson(url, options) {
    const res = await fetch(url, options);
    const data = await readJsonSafe(res);

    if (!res.ok) {
        const msg = (data && data.error) ? String(data.error) : "Request failed";
        throw new Error(msg);
    }

    return data;
}

let retentionPolicyCache = null;
async function getRetentionPolicy() {
    if (retentionPolicyCache) return retentionPolicyCache;
    retentionPolicyCache = apiJson(API.retention, { cache: "no-store" });
    return retentionPolicyCache;
}

// =============================
// Clipboard helpers
// =============================
async function copyText(text) {
    try {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return true;
        }
    } catch {
        /* ignore */
    }

    // Fallback for older browsers / non-secure contexts
    const ta = document.createElement("textarea");
    ta.value = text;
    ta.style.position = "fixed";
    ta.style.left = "-10000px";
    ta.style.top = "-10000px";
    document.body.appendChild(ta);
    ta.focus();
    ta.select();
    try {
        const ok = document.execCommand("copy");
        document.body.removeChild(ta);
        return ok;
    } catch {
        document.body.removeChild(ta);
        return false;
    }
}

async function copyFrom(el) {
    const text = (el.textContent || "").trim();
    if (!text) return;

    const ok = await copyText(text);

    // small, non-invasive feedback
    const old = el.textContent;
    el.textContent = ok ? (old + "  ✅ Copied") : (old + "  ❌ Copy failed");
    setTimeout(() => { el.textContent = old; }, 900);
}

// =============================
// Crypto helpers (AES-256-GCM, key in URL hash)
// =============================
async function encryptToPayload(plaintext) {
    const keyBytes = randBytes(32);
    const iv = randBytes(12);

    const key = await crypto.subtle.importKey("raw", keyBytes, { name: "AES-GCM" }, false, ["encrypt"]);

    const ct = new Uint8Array(
        await crypto.subtle.encrypt({ name: "AES-GCM", iv }, key, utf8.toBytes(plaintext))
    );

    const payload = {
        v: 1,
        alg: "A256GCM",
        iv: bytesToB64url(iv),
        ct: bytesToB64url(ct),
    };

    return {
        payloadString: JSON.stringify(payload),
        keyB64url: bytesToB64url(keyBytes),
    };
}

async function decryptFromPayloadString(payloadString, keyB64url) {
    let payload;
    try { payload = JSON.parse(payloadString); }
    catch { throw new Error(NOT_FOUND); }

    if (!payload || payload.v !== 1 || payload.alg !== "A256GCM" || !payload.iv || !payload.ct) {
        throw new Error(NOT_FOUND);
    }

    const iv = b64urlToBytes(payload.iv);
    const ct = b64urlToBytes(payload.ct);
    const key = await importAesKeyFromB64url(keyB64url, ["decrypt"]);

    const pt = new Uint8Array(await crypto.subtle.decrypt({ name: "AES-GCM", iv }, key, ct));
    return utf8.fromBytes(pt);
}

// =============================
// ID parsing / link building
// =============================
function parseCompositeId(input) {
    // Accept:
    //  - "id"
    //  - "id#k=KEY"
    //  - full link ".../read.html?id=ID#k=KEY"
    const s = (input || "").trim();
    if (!s) return { id: "", key: "" };

    if (/^https?:\/\//i.test(s)) {
        try {
            const u = new URL(s);
            const id = (u.searchParams.get("id") || "").trim();
            const hp = new URLSearchParams((u.hash || "").replace(/^#/, ""));
            const key = (hp.get("k") || "").trim();
            return { id, key };
        } catch {
            /* fall through */
        }
    }

    const [idPart, ...hashParts] = s.split("#");
    const id = (idPart || "").trim();
    if (!hashParts.length) return { id, key: "" };

    const hp = new URLSearchParams(hashParts.join("#"));
    const key = (hp.get("k") || "").trim();
    return { id, key };
}

function buildReadLink(id, key) {
    const url = new URL("/read.html", window.location.origin);
    url.searchParams.set("id", id);
    url.hash = `k=${encodeURIComponent(key)}`;
    return url.toString();
}

// =============================
// Actions
// =============================
async function createNote() {
    setResult(els.createResult, null, "");
    hideShare();

    const note = (els.createNote.value || "").trim();
    if (!note) {
        setResult(els.createResult, "error", "Secret is required");
        return;
    }

    setBusy(els.createBtn, true);

    try {
        const enc = await encryptToPayload(note);

        const data = await apiJson(API.create, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ note: enc.payloadString }),
        });

        const id = String(data.id || "").trim();
        if (!id) throw new Error("Failed to create note");

        els.createNote.value = "";
        els.createNote.focus();

        const compositeId = `${id}#k=${enc.keyB64url}`;
        const link = buildReadLink(id, enc.keyB64url);

        setResult(els.createResult, "success", "Secret created! Share it using composite ID or link below:");
        showShare({ compositeId, link });

        try {
            const policy = await getRetentionPolicy();
            const daysToKeep = Number(policy?.daysToKeep);
            if (Number.isFinite(daysToKeep) && daysToKeep !== -1) {
                els.retentionNote.textContent = `Your secret note will be automatically deleted after ${daysToKeep} day(s).`;
                els.retentionNote.style.display = "block";
            } else {
                els.retentionNote.style.display = "none";
                els.retentionNote.textContent = "";
            }
        } catch {
            els.retentionNote.style.display = "none";
            els.retentionNote.textContent = "";
        }
    } catch (err) {
        setResult(els.createResult, "error", err?.message || String(err));
    } finally {
        setBusy(els.createBtn, false);
    }
}

async function readNote() {
    setResult(els.readResult, null, "");

    const { id, key } = parseCompositeId(els.readId.value);
    if (!id) {
        setResult(els.readResult, "error", "ID is required");
        return;
    }

    setBusy(els.readBtn, true);

    try {
        if (!key) throw new Error(NOT_FOUND);

        // DELETE returns encrypted payload, then we decrypt locally
        const data = await apiJson(API.consume(id), { method: "DELETE" });

        const payloadString = String(data.note || "");
        if (!payloadString) throw new Error(NOT_FOUND);

        const plaintext = await decryptFromPayloadString(payloadString, key);
        setResult(els.readResult, "success", `Secret:\n\n${plaintext}`);
    } catch (err) {
        // Normalize “not found” and crypto errors to the same message
        const msg = err?.message || String(err);
        setResult(els.readResult, "error", msg.includes("AES") ? NOT_FOUND : msg);
    } finally {
        setBusy(els.readBtn, false);
    }
}

// =============================
// Wire up events (no inline JS)
// =============================
els.createBtn.addEventListener("click", createNote);
els.readBtn.addEventListener("click", readNote);

els.copyIdBtn.addEventListener("click", () => copyFrom(els.guidBox));
els.copyLinkBtn.addEventListener("click", () => copyFrom(els.linkBox));

// Nice-to-have: Ctrl/Cmd+Enter submits
els.createNote.addEventListener("keydown", (e) => {
    if ((e.ctrlKey || e.metaKey) && e.key === "Enter") createNote();
});
els.readId.addEventListener("keydown", (e) => {
    if (e.key === "Enter") readNote();
});

document.addEventListener("DOMContentLoaded", () => {
     getRetentionPolicy().catch(() => { /* ignore */ });
});
