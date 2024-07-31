# Timberborn modding tools and examples

## Important: version compatibility note

This project requires Timberborn in version <b>0.6.0.0</b> or higher, also known as <b>Update 6</b>. This means that for the time being it is only compatible with the experimental version of the game.

## Overview
This project's intent is to make you familiar with the Timberborn modding pipeline. It contains three example mods that showcase basic modding possibilities - adding new buildings, adding your own scripts and overwriting existing game elements. There are also the tools for automatic Timberborn DLLs import and building the mods.

The repository also contains Timbermesh editor libraries, which allow you to use our [custom mesh format](https://github.com/mechanistry/timbermesh).

You can learn more about modding Timberborn from this article, as well as from the [community wiki](https://timberborn.wiki.gg/wiki/Creating_Mods_(Update_6)).

The best place to discuss the experimental build, as well as modding in general, is Timberborn's official Discord server: [https://discord.gg/timberborn](https://discord.gg/timberborn).

## Unity quick start
1. Download Unity in the same version as specified in [this file](https://github.com/mechanistry/timberborn-modding/blob/main/ProjectSettings/ProjectVersion.txt).
   - Make sure you install Unity Windows Build Support and Mac Build Support.
2. Clone this project.
3. Add the project to Unity Hub but do not open it yet.
4. In Unity Hub, add `-disable-assembly-updater` as a command line argument to the project.
5. Open the project, do not enter Safe Mode - select Ignore from prompt.
6. You should see a prompt asking you to import Timberborn DLLs. Select the folder where your Timberborn is installed.
   - You can always trigger DLLs import manually, by selecting "Timberborn" > "Import Timberborn dlls" from the toolbar.
7. Wait for the import to complete and the project to open.
8. In the toolbar click "Timberborn" > "Show Mod Builder". There you can choose whether you want to build all or selected mods found in the project.
   - The mod builder places the resulting files in `Documents/Timberborn/Mods`, including your code and asset bundles.
   - You need to have Mac Build Support module installed, as the builder creates AssetBundles for both Windows and MacOS versions of the game.

If you start Timberborn, you should now see your mod in the mod manager.

Keep in mind that many mods do not require Unity. For example, modifying existing specifications or adding new translations can be done without Unity. Keep reading to find out more.

## Installing modding tools only (advanced)
Instead of cloning this repository and using it as a base for your mods as described above, you can install the modding tools only. To do so, open the Package Manager in your Unity project and add the following package using the "Add package from git URL" option: `https://github.com/mechanistry/timberborn-modding.git?path=/Assets/Tools`

Remember to update the package to the latest version from time to time. If you want, you can switch to a certain branch or commit using advanced options described in the Unity documentation: https://docs.unity3d.com/Manual/upm-git.html#revision

## Mod manager
Timberborn includes a basic built-in mod manager. The mod manager is shown every time the game starts if it detects any mods in `Documents/Timberborn/Mods` or mods downloaded by Steam Workshop. You can also open it manually from the main menu.

The mod manager allows you to activate and deactivate mods and change their loading order. By default, mods are ordered so that each mod is loaded after its dependencies.

## Basic mod structure
When working on a Timberborn mod, you store it in the `Documents/Timberborn/Mods` directory, with each mod having its own subfolder. That is also where you store mods that you downloaded manually.

Example:
```
Documents/
└── Timberborn/
    └── Mods/
        └── MyFirstMod/
            ├── AssetBundles/
            │   └── ModAssets.assets
            ├── Localizations/
            │   └── enUS_myMod.csv
            ├── Materials/
            │   └── Beavers/
            │       └── Folktails/
            │           └── Adult/
            │               ├── BeaverAdult1.Folktails.png
            │               ├── BeaverAdult2.Folktails.png
            │               └── BeaverAdult3.Folktails.png
            ├── Specifications/
            │   ├── Goods/
            │   │   ├── GoodSpecification.Berries.json
            │   │   └── GoodSpecification.Moonshine.json
            │   └── Recipes/
            │       └── RecipeSpecification.Antidote.json
            ├── Sprites/
            │   └── Goods/
            │       ├── MoonshineIcon.png
            │       └── MoonshineIcon.png.meta.json
            ├── Code.dll
            └── manifest.json
```

## Manifest
Each mod has a `manifest.json` file in its root folder which looks as follows.

```
{
  "Name": "My First Mod",
  "Version": "0.1",
  "Id": "MyFirstMod",
  "MinimumGameVersion": "0.0.0.0",
  "Description": "The very first mod.",
  "RequiredMods": [
    {
      "Id": "AnotherMod",
      "MinimumVersion": "0.1"
    }
  ],
  "OptionalMods": [
    {
      "Id": "YetAnotherMod"
    }
  ]
}
```

RequiredMods and OptionalMods are dependecies for your mod, meaning the game will load them before your mod. The difference between them is that RequiredMods will trigger a warning icon in the mod manager if they are missing, while OptionalMods will not.

## Specifications
You can modify and extend many aspects of the game without using code, Unity, or other mods by simply placing a `.json` file in the correct folder. We call those files Specifications.

For example, the game stores the Specification of the need Hunger in this file:
```
Specifications/Needs/NeedSpecification.Beaver.Hunger.json
```

If your mod contains a JSON file under a different path than any existing Specification, it is treated as a new Specification, for example a new need or a new good.

If your mod contains a JSON file under the same path as an existing Specification, the mod's version modifies the original as follows:
* Fields with the same name replace the original values.
* Any fields that are omitted retain their original values.
* By default, list fields are overwritten by the mod's version just like any other field.
* List fields with a `#append` postfix add its elements to the original list.
* List fields with a `#remove` postfix remove the specified elements.

For example, if you place the following in `Specifications/Goods/GoodSpecification.Carrot.json`, carrots will become twice as heavy but will satisfy thirst in addition to hunger:
```
{
  "ConsumptionEffects#append": [
    {
      "NeedId": "Thirst",
      "Points": 0.3
    }
  ],
  "Weight": 2
}
```

Finally, if the file name ends with `.optional.json` for example `NeedSpecification.Beaver.Sport.optional.json`, it modifies an existing Specification only if it id already present in the base game or a different mod but is otherwise ignored. This allows you to support compatibility with other mods as well as older versions of the game.

You can use Specifications to add new or modify existing:
* Factions
* Needs
* Goods
* Recipes
* Key bindings
* And more, the list constantly evolves.

## Translations and in-game text
Timberborn uses CSV files for storing in-game texts and localization files. The official languages are stored in files named after the two-letter language code followed by a two letter country or dialect code and an optional suffix, for example:
```
enUS.csv
enUS_donottranslate.csv
enUS_names.csv
ptBR.csv
ptBR_donottranslate.csv
ptBR_names.csv
```

Each file consists of three columns:
```
ID,Text,Comment
Beaver.Age,Age: {0},"{0} stands for number of days, for example 'Age: 5'."
Beaver.Homeless,Homeless,Signifies that the beaver has no home
...
```

We call each of the entries a loc key. Loc keys can be accessed in multiple places in the project including code, json specifications and UI.

If you wish to add new content to the game in existing languages, you can add new loc keys by placing them in a new file named after the official file, followed by an underscore and a postfix of your choice. For example, if you wish to add new English texts, such as the name of a new food type, you can place them here:
```
MyFirstMod/Localizations/enUS_MyFirstMod.csv
```

If you wish to add a new language to the base game, translate each of the original English files and place them in files following the same naming convention. A hypothetical Norwegian translation would likely comprise of:
```
MyFirstMod/Localizations/noNO.csv
MyFirstMod/Localizations/noNO_donottranslate.csv
MyFirstMod/Localizations/noNO_names.csv
```

## Images
Any `.png` and `.jpg` files placed in the mod's directory can be accessed from code just like built-in images. A mod's image overrides an existing image if it has the same path.

You can control how an image is imported by the game by adding a meta file with a `.meta.json` postfix. For example, if the image is `Sprites/MyButton.png`, its meta file should be named `Sprites/MyButton.png.meta.json`. The meta file's format is:
```
{
  "isSprite": false,
  "isNormalMap": false,
  "linear": false,
  "generateMipmap": true,
  "mipmapCount": -1,
  "ignoreMipmapLimits": false,
  "filterMode": "Trilinear",
  "wrapMode": "Repeat",
  "textureFormat": "RGB32",
  "anisoLevel": 16,
  "width": 1,
  "height": 1
}
```
If a field is not present in the meta file, it is set to its default value from the above example.

## 3D models
Timberborn stores models in a custom file format called Timbermesh documented [here](https://github.com/mechanistry/timbermesh).

## AssetBundles
All AssetBundles located in the mod's `AssetBundles` subfolder are loaded by the game.

If an AssetBundle file name ends with `_win` or `_mac`, for example `MyFirstMod_win.asset`, that AssetBundle is only loaded on the corresponding operating system.

Note that many types of assets can be added or modified without using AssetBundles or Unity by simply placing them in the correct subfolder as explained in other sections of the documentation. However, files packaged in an AssetBundle follow the same rules as files placed directly in the mod's folder. For example, they can add new JSON Specifications or modify existing ones.

Some assets, notably Prefabs and sounds, can only be added by placing them in an AssetBundle.

## Code: loading DLLs
All DLLs located in the mod’s folder and its subfolders are loaded when launching the game.

The game then searches for all implementations of the `IModStarter` interface, creates instances of these implementations and runs the `StartMod` method on them. This interface lets you run your own code when the game first starts, for example apply code modifications using Cecil, Harmony or similar without having to use BepInEx or modifying the game’s Program Files directory in any other way.

The class which implements `IModStarter` must have a parameterless constructor.

You can use the `IModEnvironment` parameter to access the mod's directory on the disk.

Example:
```
public class MyFirstModStarter : IModStarter {

  public void StartMod(IModEnvironment modEnvironment) {
    // Your code goes here
  }

}
```

## Code: configurators
Timberborn uses Bindito, our dependency injection framework. The game finds and installs all implementations of `IConfigurator` annotated with the `Context` attribute which specifies which scene the configurator should be installed in. Valid parameters are: `"MainMenu"`, `"Game"` and `"MapEditor"`.

Example:
```
using Bindito.Core;

[Context("Game")]
public class ConfiguratorTest : IConfigurator {

  public void Configure(IContainerDefinition containerDefinition) {
    containerDefinition.Bind<HelloWorldNotification>().AsSingleton();
  }

}
```

Similarly, PrefabConfugurators are automatically installed if they are placed in a prefab with a name ending with `.configurator` and are automatically loaded if they are annotated with the `Context` attribute.

## Code: Harmony and Mono.Cecil
You will likely want to use Harmony or Mono.Cecil to inject your code into the game.

Using BepInEx for developing your mod is OK but requiring players to install it is discouraged. Mods relying on BepInEx are one of the most common causes of Timberborn crashes and they are notorious for being difficult to remove, they cannot be easily switched off, and they survive a full reinstallation of the game.
