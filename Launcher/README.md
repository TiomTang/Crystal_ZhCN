# Launcher configuration

`Launcher.ini.example` is documentation, not a live configuration. It is not copied to build output and does not replace an existing operator file. Create your own root `Launcher.ini` with real values before publishing it through the existing patch publisher.

| Section / key | Meaning |
| --- | --- |
| Launcher / Host | Base HTTP(S) patch URL; trailing slash is accepted or added in memory. Empty disables update requests. |
| Launcher / PatchFile | Existing publisher list name, normally `PList.gz`; the original binary format and payload URL rules are unchanged. |
| Launcher / ServerName | Launcher title/server label. |
| Launcher / Browser | Announcement URL; empty disables the browser. |
| Launcher / NeedLogin, Login, Password | Optional patch HTTP Basic authentication. These are operator distribution credentials, **not game account credentials**; use HTTPS and do not distribute a private administrative password. |
| Network / IPAddress, Port | Game endpoint, read-only in the player settings UI. IPv4, IPv6 and DNS host names are supported; port range 1–65535. |

The application never creates or saves `Launcher.ini`. The existing updater may replace only this exact root INI; nested `Launcher.ini` and all other INI files remain protected. Updated publishing settings are read with a fresh reader after completion and before game launch. The in-flight update retains its original host/list/authentication snapshot.

`Launcher.User.ini` is local personal state: `[Graphics] Resolution`, `ResolutionHeight`, `DisplayMode` (0 windowed, 1 maximized, 2 fullscreen), `FPSCap`, `AlwaysOnTop`; `[Launcher] AutoStart`, `TestConfig`; `[Game] Language`. It is saved locally and never replaced by patch entries. Game account/password state stays in game configuration, never in process arguments.

Only when `Launcher.User.ini` does not exist, personal settings are migrated per key: existing personal keys in the old `Launcher.ini` take precedence; missing keys fall back to the selected `Mir2Config.ini` / `Mir2Test.ini`. The source files are not changed. Old personal keys left in `Launcher.ini` are ignored after that first migration. Do not remove them by rewriting a player's old file. To reset personal preferences deliberately, back up and remove the local `Launcher.User.ini` yourself.

If no operator file exists, defaults remain in memory (loopback game endpoint, no patch or announcement URL); the program does not contact an example or public update site. It may create the separate personal INI. File freshness still compares the original manifest length and timestamp, **not content hashes**. Normal checks intentionally do not hash unchanged resources; use **Repair files** to download every managed entry again, including corruption with identical size and timestamp. The confirmation warns about bandwidth. Personal settings and unknown files are never cleaned by repair.

## Transaction and recovery

Downloads are streamed into `.mir2-transaction`, with HTTP, compressed-byte/expanded-length and gzip trailer checks. All required files are staged before any installed file is changed. A free-space budget includes staging and rollback backups. Download cancellation leaves installed files unchanged; application and rollback run outside the UI and must not be interrupted deliberately.

The first journal is flushed and renamed inside a separate `.mir2-initializing-<id>` directory before that directory is atomically published as `.mir2-transaction`. An interruption before publication cannot block game startup or a later update. Recovery removes an unpublished directory only when its sealed, empty Downloading journal matches the directory ID and it contains no other files. Empty directories, partial `journal.next` files, invalid journals and unknown contents are preserved without being treated as active transactions; do not recursively delete them. Older `.mir2-transaction` directories without a valid journal remain fail-closed and require manual inspection.

Before creating a rollback scratch copy, the sealed journal records ownership intent. Recovery may rebuild that copy only while the backup still matches the old hash, the installed target still matches this transaction, and existing scratch bytes are a prefix of that backup. Unowned or changed scratch files are retained rather than deleted.

A helper with its complete runtime dependencies is copied outside the installation. After the original launcher exits, it acquires the installation's exclusive lease and applies the whole set, including Launcher, Shared and Bootstrap. Original files are backed up and flushed before atomic per-file replacement. A persistent, atomically written journal records intent before replacement. Uncommitted interruptions roll back, including removal of newly created targets only when their content matches this transaction. Unknown artifacts, changed targets, invalid paths, reparse points and changed backups stop recovery and retain evidence. The game refuses to start while transaction recovery is unresolved.

Local SHA256 identifies staged/backup bytes for recovery only. The old publisher supplies no remote cryptographic hash; this is not patch authenticity verification. The journal is sealed with Windows DPAPI, bound to the installation path and original Windows account. Keep that account available for automatic recovery.

Commit requires a separate invocation of the new launcher to read its configuration, load its dependencies, construct its main form, and write a token acknowledgement. A successful `Process.Start` alone is not success. This startup probe is not a game login, browser/WebView runtime, or long-term application health guarantee. After commit the normal launcher restarts and checks again before launching the game.

If both the updater helper and original process are terminated after the installed executable has been replaced, the installed entry point may not work. Read `.mir2-transaction/RECOVERY.txt` for the external helper location and run, using the original Windows account:

```text
"<external-helper-directory>/Launcher.exe" --recover-update "<installation-directory>"
```

The external helper works independently of installed Launcher/Shared files. Successful helpers remove only their own marked, unchanged dependency copies after exit; failed helpers and recovery evidence are retained. Do not delete `.mir2-transaction` or its backups when recovery is unresolved. If the original helper was manually removed or the Windows account's DPAPI keys are lost, automatic recovery is not guaranteed; preserve the installation and obtain assistance rather than deleting backups.

Old `.launcher-update/pending.json` batches are rejected with a manual recovery message and left untouched; they are not silently migrated or deleted. Process-kill recovery tests do not constitute a real hard-power-loss guarantee: hardware, filesystem and SMB persistence behavior remain outside that claim.
