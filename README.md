# Sacred2Recomp

Unofficial experimental native recompilation of **Sacred 2: Fallen Angel (Xbox 360)** for Windows x64 using ReXGlue and Vulkan.

## Status — early alpha
Gameplay has been reached in local testing using Vulkan FSI. Graphical glitches and intermittent stutter remain; shader compilation is a suspected but unconfirmed cause. Full campaign, saves, local co-op and compatibility across hardware are not yet validated.

## Building and playing
This project requires your own legally obtained Xbox 360 game files, including `default.xex` and `s2xserver.xex`. No original game assets, extracted executables or generated game code are included. Install ReXGlue v0.11.0 and its documented build prerequisites: https://github.com/rexglue/rexglue-sdk . The initial build manifest and host scaffolding are provided as a starting point; the local runtime modifications and reproducible build procedure still need to be published and validated.

The locally tested launch options are `--gpu.driver=vulkan --gpu.vulkan.render_target_path=fsi` and `--title.game_root=<path-to-extracted-game>`.

## Roadmap
- Correct visual defects and investigate shader/pipeline stutter.
- Validate campaign progression, saves and local co-op.
- Improve performance and reproducibility.

## Português (Brasil)
Projeto experimental e não oficial de recompilação nativa de Sacred 2: Fallen Angel para Windows x64. O jogo já chegou ao gameplay em testes locais, mas apresenta defeitos gráficos e travadas ocasionais. Cada usuário deve fornecer sua própria cópia legítima. A reprodução completa do build e a distribuição de componentes permitidos ainda estão em preparação.

## Credits and rights
Thanks to the [ReXGlue](https://github.com/rexglue/rexglue-sdk), Xenia and XenonRecomp contributors. Sacred 2 and its original content belong to their respective rights holders. This project is not affiliated with or endorsed by them. Third-party components retain their respective licenses. No license is granted to original game content.

## Experimental graphical launcher
See [launcher documentation](launcher/README.md). Build with dotnet build launcher/Sacred2Launcher.csproj -c Release. End-to-end installation remains unvalidated.
