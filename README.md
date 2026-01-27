# 🌐 Language / 语言选择
- [English](#English)
- [中文](#中文) · [🇨🇳 中文版](./README_CN.md)

---

## English

> [!IMPORTANT]
> I'm currently a high school student, with studies coming first!
> Responses to issues/PRs, frequency of releasing may be slow (usually 1-4 weeks).
> Feel free to submit PRs to help fix bugs or discuss in Discussions.
> Thanks for your understanding and support! 🚀

# Mate-Engine-Linux-Port
This is an **unofficial** Linux port of shinyflvre's MateEngine - A free Desktop Mate alternative with a lightweight interface and custom VRM support.
Tested on:
- Ubuntu 24.04 LTS.
- Fedora Workstation 43 (Plasma)

![](https://raw.githubusercontent.com/Marksonthegamer/Mate-Engine-Linux-Port/refs/heads/main/Screenshot.png)

### Usage
Simply grab a prebuilt one in [Releases](https://github.com/Marksonthegamer/Mate-Engine-Linux-Port/releases/) page. Then, run the `launch.sh` script in the output directory (This script is necessary for window transparency. For KDE, you also need to **disable "Allow applications to block compositing"** in `systemsettings`).

### Requirements
- A common GNU/Linux distro
- A common X11 or Wayland desktop environment which supports compositing (such as KDE, Xfce, GNOME)
- At least 1 GiB of swap space (optional)
- `libpulse` and `pipewire-pulse` (if you are using Pipewire as audio server)
- `libgtk-3-dev libglib2.0-dev libayatana-appindicator`
- `libx11-6 libxext6 libxrender1 libxdamage1`

On Ubuntu and other Debian-based Linux:
```bash
sudo apt install libpulse-dev libgtk-3-0t64 libglib2.0-0t64 libayatana-appindicator3-1 libx11-6 libxext6 libxrender1 libxdamage1
```
On Fedora:
```bash
sudo dnf install pulseaudio-libs gtk3 glib2 libX11 libXext libXrender-devel libXdamage libayatana-appindicator-gtk3
```
On Arch Linux:
```bash
sudo pacman -S libpulse gtk3 glib2 libx11 libxext libxrender libxdamage libayatana-appindicator
```

Note that if you use GNOME, you will need [AppIndicator and KStatusNotifierItem Support extension](https://extensions.gnome.org/extension/615/appindicator-support/) to show tray icon.

### How to build / compile
- For security reasons, you need to compile StandaloneFileBrowser plugin manually (just use `make` command under `Mate-Engine-Linux-Port/Plugins/Linux/StandaloneFileBrowser`, then copy `libStandaloneFileBrowser.so` to `Mate-Engine-Linux-Port/Assets/MATE ENGINE - Packages/StandaloneFileBrowser/Plugins/Linux/x86_64`)
- Then open the project in Unity 6000.2.6f2 and build the player with executable name "MateEngineX.x86_64"

### Ported Features & Highlights
- Model visuals, alarm, screensaver, Chibi mode (they always work, any external libraries are not required for them)
- Transparent background with cutoff
- Set window always on top
- Dancing (experimental, require `pulseaudio` or `pipewire-pulse` for audio program detection)
- AI Chat (require `Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf`, case-sensitive)
- Mouse tracking (hand holding and eyes tracking)
- Discord RPC
- Custom VRM importing
- Simplified Chinese localization
- Event-based Messages
- Lower RAM usage than Windows version (Memory trimming enabled)

### Synthetic Heart integration
<img src="https://raw.githubusercontent.com/XargonWan/Synthetic_Heart/develop/docs/res/synth_banner.png" alt="Synthetic Heart" width="240" />
Bring your virtual companion to the desktop — **Synthetic Heart (SyntH)**. This optional plugin (located at `Plugins/Synthetic_Heart`) connects MateEngine with SyntH and enables easy features like animation sharing, prompt hints and message routing. 

Is it included in releases? If the `Plugins/Synthetic_Heart` folder exists in this repository it will be packaged into our release tarball and placed into the runtime layout (both `Plugins/Synthetic_Heart` and `MateEngineX_Data/StreamingAssets/Mods/Synthetic_Heart`). If you don't find it in your release, you can install it manually.

How to install & connect:
- From a release: extract the tarball — plugin files will already be included when present in the repo.
- Manual install: copy the `Plugins/Synthetic_Heart` folder into the game root (next to `MateEngineX.x86_64`) or into `MateEngineX_Data/StreamingAssets/Mods/Synthetic_Heart`, then restart the game.
- Run a Synthetic Heart server (see https://github.com/XargonWan/Synthetic_Heart). The plugin talks to SyntH's web API (default `http://localhost:11434`).
- Quick connectivity check: start the game with the argument `--synth-integration-test=http://<synth-host>:11434`; the integration runner will probe `GET /api/prompt_override` and exit with code 0 on success.

Learn more and install SyntH: https://github.com/XargonWan/Synthetic_Heart

![](https://raw.githubusercontent.com/Marksonthegamer/Mate-Engine-Linux-Port/refs/heads/main/RAMComparition.png)

### Known Issues
- Window snapping and dock sitting are still kind of buggy, and they don't work on XWayland Interface
- Crashes at low system performance (`pa_mainloop_iterate`)
- Limited window moving in Mutter (GNOME)
- PulseAudio sometimes returns an empty audio program name
- Mods do not load correctly
- Sometimes is hard to focus the Mate or its menu via mouse control

### Removed
- Steam API (no workshop support)
- NAudio
- UniWindowController

This project lacks further testing and updates. Feel free to make PRs to contribute!

