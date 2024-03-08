# Mare's BSP Asset Detector
Terminal application that detects custom assets inside Source Engine maps that need packing and writes them all in a file list ready to use with Valve's bspzip. Code is based and taken from [CompilePal](https://github.com/ruarai/CompilePal), converted to be a simple CLI application. This is designed to be used by another program (Mare's BSP Compiler) and not directly, but its functional by itself. Also, this program primarily targets Linux users, so it has changes meant to work for it best, but it might probably work on Windows, althought you are better off using CompilePal itself instead of using this.

Fun fact: This can detect mare pupil textures properly unlike CompilePal! At least that I remember from the last release version (V028).

*NOTE: This program is currently considered a beta and not stable.*

## Usage
Simply pass your unpacked map's .BSP and the game's folder you're using assets from (e.g. for TF2, the 'tf' folder).
```
AssetDetector "<map BSP>" "<game folder>"
```
#### Optional Arguments:
* `-o "<output file>"` -- Output the filelist with a different name to a different directory instead of the default `detected_assets.txt` in the program's root.
* `-verbose` -- Verbose output.
* `-renamenav`
* `-no-swvtx` -- Do not include .sw.vtx files.

## Compiling
Project was built using .NET 8.0, but I'm pretty sure you can use .NET 6.0 at the earliest. Make sure to have `dotnet` installed for whichever Linux distro you use.

Use one of these commands in the `src` directory.
```
dotnet build           # Debug build
dotnet publish         # Release build
```
