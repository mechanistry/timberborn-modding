using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools {
  internal class ModFinder {

    private static readonly string ModsDirectory = "Assets/Mods";
    private static readonly string ModManifest = "manifest.json";
    private readonly ModBuilderControlsPersistence _modBuilderControlsPersistence = new();

    public IEnumerable<ModDefinition> GetEnabledMods() {
      foreach (var modDefinition in GetAllMods()) {
        if (_modBuilderControlsPersistence.IsModEnabled(modDefinition)) {
          yield return modDefinition;
        }
      }
    }

    public IEnumerable<ModDefinition> GetAllMods() {
      var projectPath = Path.GetDirectoryName(Application.dataPath);
      foreach (var modFolder in GetModDirectories()) {
        var manifestPath = $"{modFolder}/{ModManifest}";
        var manifestAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(manifestPath);
        if (manifestAsset) {
          yield return new(modFolder.Replace(ModsDirectory + "/", ""),
                           modFolder, $"{projectPath}/{modFolder}");
        }
      }
    }

    public IEnumerable<string> GetModDirectories() {
      foreach (var modFolder in AssetDatabase.GetSubFolders(ModsDirectory)) {
        yield return modFolder;
      }
    }

  }
}