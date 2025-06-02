using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools {
  [InitializeOnLoad]
  internal static class TimberbornLibrariesChecker {

    private static readonly string LibrariesCheckedKey = "TimberbornLibrariesChecked";
    private static readonly string TimberbornDllPattern = "Timberborn.*.dll";

    static TimberbornLibrariesChecker() {
      if (!SessionState.GetBool(LibrariesCheckedKey, false)) {
        if (!LibrariesExist()) {
          ShowImportDialog();
        }
        SessionState.SetBool(LibrariesCheckedKey, true);
      }
    }

    private static bool LibrariesExist() {
      var dllPath = Path.Combine(Application.dataPath, TimberbornLibrariesImporter.DllDirectory);
      var dllDirectory = new DirectoryInfo(dllPath);
      return dllDirectory.Exists && dllDirectory.EnumerateFiles(TimberbornDllPattern).Any();
    }

    private static void ShowImportDialog() {
      if (EditorUtility.DisplayDialog("Timberborn libraries not found",
                                      "Timberborn libraries were not found in the project. "
                                      + "Would you like to open Timberborn game directory "
                                      + "and import them now?", "Yes", "No")) {
        TimberbornLibrariesImporter.Import();
      }
    }

  }
}