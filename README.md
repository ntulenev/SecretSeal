## 🦭🔐 SecretSeal

**SecretSeal** is a self-hosted, privacy-first service inspired by [privnote.com], designed for personal use and sharing secrets with friends.

It allows you to create **one-time readable messages** that are **automatically destroyed after being opened**.

![Screenshot](Screenshot.png)

SecretSeal runs on your own hosting, giving you full control over your data

### ✨ Features
- 🗝️ One-time secret messages (read once, then gone)
- 🏠 Self-hosted — you own the infrastructure and data
- 🔐 Client-side encryption and optional server-side encryption
- 💾 Flexible storage — choose between InMemory mode or a database storage
- 🚫 No ads, no tracking
- 🤝 Perfect for personal use and sharing secrets with friends

### 🔐 Encryption

#### Client-side encryption
Before sending, the note is encrypted in the browser using AES-256-GCM (Web Crypto API).
Key is generated per note and stored only in the share link’s URL fragment (#k=), so it is never sent to the server. 
The server stores only the encrypted payload (IV + ciphertext), and decryption happens only on the client when the recipient opens the link

#### Server-side encryption
In case the service is used as an API, server-side encryption is also supported.
Notes on a server side are stored **encrypted** using **AES-256**.

- Encryption key is provided via `appsettings.json`
- Plaintext data is never stored
- Protects against accidental disclosure (logs, memory dumps, debugging, crashes)

```json
  "Storage": {
    "Mode": "InMemory"
    // "Mode": "Database"
    // "Mode": "InMemory"
  },
  "Validation": {
    "MaxNoteLength": 4000 // Optional content length limitation
  },
  "ConnectionStrings": {
    "SecretSealDb": "Server=localhost\\SQLEXPRESS;..."
  },
  "Crypto": {
    "Key": "32-byte-secret-key"
  },
```

### 📊 Statistics

**GET `/stat`**

Returns basic runtime statistics.

```json
{
  "notesCount": 3,
  "encryptionEnabled": true,
  "isInMemory": true
}
```

| Field | Description |
|------|-------------|
| notesCount | Total number of stored notes |
| encryptionEnabled | Indicates whether notes are stored in the system in encrypted or plain text form |
| isInMemory | Indicates whether the application is currently using in-memory storage instead of a database |
