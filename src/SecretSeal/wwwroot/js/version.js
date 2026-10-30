// The running assembly owns the version; never duplicate it in the UI.
const versionElement = document.getElementById("projectVersion");

if (versionElement) {
    fetch("/version", { cache: "no-store" })
        .then(async (response) => {
            if (!response.ok) throw new Error("Version unavailable");
            const data = await response.json();
            if (typeof data.version !== "string" || !data.version) throw new Error("Version unavailable");
            versionElement.textContent = `v${data.version}`;
        })
        .catch(() => { versionElement.textContent = "Version unavailable"; });
}
