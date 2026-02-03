# DoomLauncher - WebView2 Integration

This is a modified version (fork) of the original **DoomLauncher** by **Hobomaster22**.

**Original Repository:** [https://github.com/Hobomaster22/DoomLauncher](https://github.com/Hobomaster22/DoomLauncher)

## 🚀 What's New in This Version?
The original launcher used an outdated Internet Explorer component which caused issues with searching and downloading from Doomworld/IdGames. This version integrates **Microsoft WebView2 (Edge)** for a modern and stable experience.

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
* **Original Creator:** [Hobomaster22](https://github.com/Hobomaster22)
* **WebView2 Integration:** [sanjiamca](https://github.com/sanjiamca)
