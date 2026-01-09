# Packaging Guide

## Version Number Locations

Update the version number in these files before releasing:

1. **FeastMaster/FeastMasterCore/FeastMaster.cs**
   - `private const string PluginVersion = "X.X.X";`

2. **manifest.json**
   - `"version_number": "X.X.X"`

3. **README.md**
   - Add new changelog entry under `### Changelog`

4. **FeastMaster/Properties/AssemblyInfo.cs**
   - `[assembly: AssemblyVersion("X.X.X.0")]`
   - `[assembly: AssemblyFileVersion("X.X.X.0")]`

## Release Package Contents

The release zip should contain:
- `FeastMaster.dll` (from `FeastMaster/bin/Release/`)
- `manifest.json`
- `README.md`
- `icon.png` (256x256)

## Build Command

```
dotnet build FeastMaster.sln -c Release
```

## Create Release Zip

```powershell
Compress-Archive -Path 'icon.png', 'manifest.json', 'README.md', 'FeastMaster/bin/Release/FeastMaster.dll' -DestinationPath 'FeastMaster-X.X.X.zip' -Force
```
