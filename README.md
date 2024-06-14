# Timberborn modding tools and examples

### Important: version compatibility note

This project requires Timberborn in version <b>0.6.0.0</b> or higher, also known as <b>Update 6</b>. This means it won't work until the Update 6 experimental release, and then it will only be compatible with the experimental version of the game for a while. For now, this project is for preview purposes only.

### Overview

This Unity project's intent is to make you familiar with the Timberborn modding pipeline. It contains three simple mods, that showcase basic modding possibilities - adding new buildings, adding your own scripts and overwriting existing game elements. There are also the tools for automatic Timberborn DLLs import and building the mods.

The repository also contains Timbermesh editor libraries, which allow you to use our [custom mesh format](https://github.com/mechanistry/timbermesh).

You can learn more about modding Timberborn from the [guide](https://timberborn.wiki.gg/wiki/Creating_Mods_(Update_6)).

### Requirements

You will need:
- Timberborn version 0.6.0.0 or higher.
- Unity in the same version as specified in [this file](https://github.com/mechanistry/timberborn-modding/blob/main/ProjectSettings/ProjectVersion.txt).
- Unity Windows Build Support and Mac Build Support.

### How to use it

0. Wait for Update 6 experimental release!
1. Add the project to Unity Hub but do not open it yet.
2. In Unity Hub, add `-disable-assembly-updater` as a command line argument to the project.
3. Open the project, do not enter Safe Mode - select Ignore from prompt.
4. You should see a prompt asking you to import Timberborn DLLs. Select the folder where your Timberborn is installed.
   - You can always trigger DLLs import manually, by selecting "Timberborn" > "Import Timberborn dlls" from the toolbar.
5. Wait for the import to complete and the project to open.
6. In the toolbar click "Timberborn" > "Show Mod Builder". There you can choose whether you want to build all or selected mods found in the project.
   - The mod builder places the resulting files in `Documents/Timberborn/Mods`, including your code and asset bundles.
   - It always deletes the current content of the mod being built.
