# DoomLauncher - WebView2 Integration

This is a modified version (fork) of the **DoomLauncher** repository by **nstlaurent**.

**Original Repository:** [https://github.com/nstlaurent/DoomLauncher](https://github.com/nstlaurent/DoomLauncher)

## 🚀 What's New in This Version?
The original launcher used components that caused issues with searching and downloading from Doomworld/IdGames on modern systems. This version integrates **Microsoft WebView2 (Edge)** for a modern and stable experience.

### Key Changes:
* **WebView2 Engine:** Replaced the legacy web browser control with Microsoft WebView2.
* **Fixed Search:** IdGames search functionality now works seamlessly with custom query support.
* **Auto-Download:** Fixed download handling directly within the launcher.
* **Dependencies:** Updated project to include necessary WebView2 NuGet packages and 7-Zip binaries (`7z.dll`).

## 🛠 Installation / Build
1.  Clone this repository.
2.  Restore NuGet packages (ensure `Microsoft.Web.WebView2` is installed).
3.  **Important:** Make sure `7z.dll` (x86 or x64) is placed next to the executable (`.exe`) for archive handling to work properly.
4.  Build and Run!

## ⚖️ Credits & License
* **Original Repository:** [nstlaurent](https://github.com/nstlaurent)
* **WebView2 Integration:** [sanjiamca](https://github.com/sanjiamca)
