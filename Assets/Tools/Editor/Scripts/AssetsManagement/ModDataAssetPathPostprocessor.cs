using JetBrains.Annotations;
using Timberborn.AssetSystem.Editor;

namespace Timberborn.ModdingTools.AssetsManagement {
  [UsedImplicitly]
  internal class ModDataAssetPathPostprocessor : IEditorAssetPathProcessor {

    private static readonly string PrefixFolder = "/Data/";

    public string ProcessForRelativePath(string path) {
      var normalizedPath = path.Replace('\\', '/');
      var prefixIndex = normalizedPath.IndexOf(PrefixFolder, System.StringComparison.Ordinal);
      if (prefixIndex >= 0) {
        return normalizedPath[(prefixIndex + PrefixFolder.Length)..];
      }
      return normalizedPath;
    }

    public string ProcessForLoad(string path) {
      return path;
    }

  }
}