# ValheimFoodConfig - Development Setup

## Prerequisites

- Visual Studio or JetBrains Rider
- .NET Framework 4.8 SDK
- Valheim installed
- [Vortex Mod Manager](https://www.nexusmods.com/site/mods/1) (optional, for testing)

## Required Tools

### ILSpy
Download from [GitHub](https://github.com/icsharpcode/ILSpy) to inspect Valheim assemblies.

### BepInEx AssemblyPublicizer
Required to make private Valheim methods accessible for patching.
Download from [GitHub](https://github.com/BepInEx/BepInEx.AssemblyPublicizer).

**Note:** Must be re-run after each Valheim or BepInEx update.

## Assembly References

Add the following references to your project:

**From `Valheim/BepInEx/core`:**
- 0Harmony.dll
- BepInEx.dll

**From `Valheim/valheim_Data/Managed`:**
- assembly_valheim.dll
- UnityEngine.dll
- UnityEngine.CoreModule.dll

## Debugging Tools

These BepInEx plugins are helpful for development (place in `BepInEx/plugins`):

| Tool | Purpose |
|------|---------|
| ConfigurationManager | Edit mod configs in-game |
| ScriptEngine | Live code reloading |
| UnityExplorer | Inspect Unity objects (press F7) |

## Troubleshooting

### BepInEx Preloader Fails (with Vortex)
Download BepInEx manually and copy these files to Valheim's base folder:
- `doorstop_config.ini`
- `start_game_bepinex.bat`
- `start_server_bepinex.bat`
- `winhttp.dll`

## Packaging for Thunderstore

1. Prepare your mod folder with:
   - `ValheimFoodConfig.dll`
   - `icon.png` (256x256)
   - `manifest.json`
   - `README.md`
2. Select all files and compress to `.zip`
3. Upload to Thunderstore
