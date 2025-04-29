# Command Palette (CmdPal) Unity Extension

Related repositories:

* [PowerToys Command Palette utility](https://github.com/microsoft/PowerToys/tree/main/src/modules/cmdpal)
* [Unity Setup Powershell Module](https://github.com/microsoft/unitysetup.powershell) (Great project but unused here)
* [Unity3D Project lister and launcher](https://github.com/falldeaf/unity-flowlauncher) (Inspired me to make this plugin)

## Installation

### Requirements
The Command Palette Unity Extension requires:
* PowerToys with Command Palette included
* Windows 11
* An ARM64 or x64 processor

### WinGet [Recommended]

`winget install maoyeedy.UnityForCmdPal `

### Microsoft Store

In Progress.

### Via GitHub

Released builds can be manually downloaded from this repository's [Releases page](https://github.com/maoyeedy/CmdPalUnityExtension/releases).


## Development

### Parsing UnityHub History
Beautify the compressed json with `jq`:
```
cat ~/AppData/Roaming/UnityHub/projects-v1.json | jq
```
List most recent 3 projects:
```
cat ~/AppData/Roaming/UnityHub/projects-v1.json | jq '.data | to_entries | .[-3:] | from_entries'
```

### Launch project bypassing UnityHub
```
& "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe" -projectPath $Path
```
