---

> [!IMPORTANT]
> 我是高二学生，学业很忙！
> Issues/PR 回复与版本发布较慢，欢迎提交 PR 或去 Discussions 讨论。谢谢理解！

> [!NOTE]
> 项目仍在维护，但优先级在高考前会降低。

# Mate-Engine-Linux-Port（中文）
这是一个非官方的 MateEngine Linux 移植版 —— 一个免费的桌面伴侣（桌宠）替代品，具有轻量界面与自定义 VRM 支持。
已在 Ubuntu 24.04 LTS 测试。

![](https://raw.githubusercontent.com/Marksonthegamer/Mate-Engine-Linux-Port/refs/heads/main/Screenshot.png)

## 用法
在 Releases 页面获取预构建版本，然后在输出目录运行 `launch.sh`（该脚本用于保证透明背景）。对于 KDE，请在系统设置中禁用“允许应用程序阻止显示特效合成”。

## 系统要求
- 常见的 GNU/Linux 发行版
- 支持合成（compositing）的 X11/Wayland 桌面（如 KDE、Xfce、GNOME）
- 建议至少 1 GiB 交换空间
- 需要的库（示例）：`libpulse`/`pipewire-pulse`、`libgtk-3`、`libglib2.0`、`libayatana-appindicator` 等（见下方安装命令示例）

### 快速安装依赖示例
Ubuntu / Debian:
```bash
sudo apt install libpulse-dev libgtk-3-0t64 libglib2.0-0t64 libayatana-appindicator3-1 libx11-6 libxext6 libxrender1 libxdamage1
```

Fedora:
```bash
sudo dnf install pulseaudio-libs-devel gtk3-devel glib2-devel libX11-devel libXext-devel libXrender-devel libXdamage-devel libayatana-appindicator
```

Arch Linux:
```bash
sudo pacman -S libpulse gtk3 glib2 libx11 libxext libxrender libxdamage libayatana-appindicator
```

> 如果你使用 GNOME，需要安装 AppIndicator 扩展以显示托盘图标： https://extensions.gnome.org/extension/615/appindicator-support/

## 构建说明
- 出于安全考虑，`StandaloneFileBrowser` 插件需手动编译（在 `Plugins/Linux/StandaloneFileBrowser` 目录运行 `make`，并将生成的 `libStandaloneFileBrowser.so` 复制到对应的 `Assets/.../Plugins/Linux/x86_64` 目录）。
- 使用 Unity（见 CI docs）打开项目并构建 Player，确保可执行名为 `MateEngineX.x86_64`。

## 功能亮点
- 支持自定义 VRM、动画与事件消息
- 透明窗口背景（带 cutoff）
- Discord RPC、鼠标跟踪、AI 聊天支持（需额外模型资源）
- 低内存占用（相对于 Windows 版本）

## Synthetic Heart 插件（SyntH）
<img src="https://raw.githubusercontent.com/XargonWan/Synthetic_Heart/develop/docs/res/synth_banner.png" alt="Synthetic Heart" width="240" />
把你的虚拟伙伴带到桌面 — **Synthetic Heart (SyntH)**。

- 包含方式：如果仓库中存在 `Plugins/Synthetic_Heart`，构建/发布时会将其包含到 release tar 中并放入运行时布局（`Plugins/Synthetic_Heart` 和 `MateEngineX_Data/StreamingAssets/Mods/Synthetic_Heart`）。如果发行版中没有包含，你也可以手动安装（见下）。

- 手动安装：将 `Plugins/Synthetic_Heart` 复制到游戏根目录（与 `MateEngineX.x86_64` 同级）或复制到 `MateEngineX_Data/StreamingAssets/Mods/Synthetic_Heart`，然后重启游戏。

- 运行 SyntH：请参考上游项目文档（https://github.com/XargonWan/Synthetic_Heart），常见方式为使用 Docker Compose 或直接运行服务。插件默认通过 `http://localhost:11434` 的 Web API 与 SyntH 通信。

- 连通性验证：可在游戏启动命令中添加参数 `--synth-integration-test=http://<synth-host>:11434`，运行后集成检测器会调用 `GET /api/prompt_override`，成功时以退出码 0 返回。

更多信息与安装指南请见上游项目： https://github.com/XargonWan/Synthetic_Heart

## 已知问题
- 窗口行为在某些桌面下仍有兼容性问题
- 部分系统在性能低时可能崩溃
- Mod 加载在少数环境下有问题

## 已删除
- Steam API（不再包含 Workshop 支持）
- NAudio
- UniWindowController

该项目缺乏进一步的测试与更新；欢迎通过 Pull Request 改进文档与代码！

