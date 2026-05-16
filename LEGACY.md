# Legacy: ProgressReleases & Installers

This repo's second commit will be the last to contain the old pre-built binary installers and installer build tooling from the original development cycle (~2007–2010).

## Historical Context

- Historically, Xenocide was developed utilizing C++ and the platform-independent Ogre 3D graphics engine.
- The project was discontinued due to the inherent challenges of volunteer-driven game development.
- Initially developed in C++, it was eventually remade in C# and XNA. Even though progress picked up steam, the bus factor was pretty low and it was eventually abandoned.
  - On August 19th, 2010, Project Xenocide Seniors announced on their website that development has been stopped due to lack of manpower.
- The codebase was migrated from **Subversion** to **Git** as a single archival commit (`e9448fe`).
- SVN revision numbers in the filenames (388, 431, 489, 638, 653, 728) map to the original commit history, which was not preserved in the Git export.
- The original project website was at `http://www.projectxenocide.com/` (now defunct).

## `assets/ProgressReleases/`

These are Windows executable installers published at various development milestones. The filenames reference the originating Subversion (SVN) revision numbers, giving a rough timeline of progress. Both full installers and smaller incremental update patches are present. The latest known build was **v0.4.1895.0**.

## `xna/Installers/`

### Current Installer (NSIS)

The newest installer used the **Nullsoft Scriptable Install System (NSIS)**. Key files:

| File | Description |
|------|-------------|
| `Xenocide Install.nsi` | Main installer script (914 lines) |
| `Includes/DotNet check.nsh` | .NET 2.0 SP1 prerequisite check |
| `Includes/XNA check.nsh` | XNA Framework 1.0 Refresh check |
| `Includes/DirectX check.nsh` | DirectX 9.0c prerequisite check |
| `Includes/bonus.nsh` | Optional bonus content (audio pack, high-res textures) |
| `Installers/xnafx_redist.msi` | Bundled XNA Framework 3.0 Refresh (~7.6 MB) |
| `Installers/dxwebsetup.exe` | DirectX web installer |
| `Needed Plugins/` | NSIS plugin DLLs (GetVersion, HwInfo, ZipDLL) |

The installer validated system requirements (Windows version, CPU, RAM, GPU), installed core game files (CeGui DLLs, compiled XNB content, XML data, textures, models, sounds), created Start Menu/Desktop shortcuts, and wrote uninstaller entries.

### Old Inno Installer (Legacy)

The predecessor installer system used **Inno Setup**:

| File | Description |
|------|-------------|
| `xenocide.iss` | Inno Setup script |
| `pre-install-info.txt` | Pre-installation instructions |
| `dxxna.exe` | Custom installer helper |
| `dxwebsetup.exe` | DirectX web installer |
| `xnafx_redist.msi` | XNA Framework 1.0 Refresh (~2.1 MB) |
