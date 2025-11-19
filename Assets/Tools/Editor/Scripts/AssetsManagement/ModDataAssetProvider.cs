using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Timberborn.AssetSystem;
using Timberborn.ModdingTools.Common;
using Timberborn.ModdingTools.ModBuilding;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools.AssetsManagement {
  [UsedImplicitly]
  internal class ModDataAssetProvider : IAssetProvider {

    private readonly ModFinder _modFinder = new();

    public bool IsBuiltIn => false;

    public bool TryLoad<T>(string path, out OrderedAsset orderedAsset) where T : Object {
      orderedAsset = LoadAll<T>(path).FirstOrDefault();
      return orderedAsset.Asset != null;
    }

    public IEnumerable<OrderedAsset> LoadAll<T>(string path) where T : Object {
      var pathDirectory = Path.GetDirectoryName(path) ?? string.Empty;
      var fileName = Path.GetFileName(path) + ".*";
      foreach (var modDirectory in _modFinder.GetModDirectories()) {
        var modPath = Path.Combine(modDirectory, AssetsCopier.DataDirectory, pathDirectory);
        if (Directory.Exists(modPath)) {
          foreach (var asset in LoadFromDirectory<T>(modPath, fileName)) {
            yield return new(0, asset);
          }
        }
      }
    }

    public void Reset() {
    }

    private static IEnumerable<T> LoadFromDirectory<T>(string modPath, string fileName)
        where T : Object {
      var matchingFiles = Directory.GetFiles(modPath, fileName, SearchOption.TopDirectoryOnly);
      foreach (var matchingFile in matchingFiles) {
        if (!matchingFile.EndsWith(".meta")) {
          var asset = AssetDatabase.LoadAssetAtPath<T>(matchingFile);
          if (asset != null) {
            yield return asset;
          }
        }
      }
    }

  }
}