using System.IO;
using System.Linq;
using UnityEditor;

namespace Timberborn.ModdingTools.AssetsImporting {
  [InitializeOnLoad]
  internal static class TimberbornAssetsChecker {

    private static readonly string AssetsCheckedKey = "TimberbornAssetsChecked";
    private static readonly string TimberbornDllPattern = "Timberborn.*.dll";

    static TimberbornAssetsChecker() {
      if (!SessionState.GetBool(AssetsCheckedKey, false)) {
        if (!LibrariesExist()) {
          ShowImportDialog();
        }
        SessionState.SetBool(AssetsCheckedKey, true);
      }
    }

    private static bool LibrariesExist() {
      var dllDirectory = new DirectoryInfo(TimberbornAssetsImporter.DllPath);
      var assetsDirectory = new DirectoryInfo(TimberbornAssetsImporter.ImportedAssetsPath);
      return dllDirectory.Exists
             && dllDirectory.EnumerateFiles(TimberbornDllPattern).Any()
             && assetsDirectory.Exists
             && assetsDirectory.EnumerateFiles().Any();
    }

    private static void ShowImportDialog() {
      if (EditorUtility.DisplayDialog("Timberborn libraries not found",
                                      "Timberborn libraries were not found in the project. "
                                      + "Would you like to open Timberborn game directory "
                                      + "and import them now?", "Yes", "No")) {
        TimberbornAssetsImporter.Import();
      }
    }

  }
}