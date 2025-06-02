using System.IO;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools {
  internal class TimberbornLibrariesFinder {

    private static readonly string MacNotFoundMessage =
        "File wasn't recognized as Timberborn build. Make sure you selected Timberborn app file.";
    private static readonly string WindowsNotFoundMessage =
        "Directory wasn't recognized as Timberborn build directory. "
        + "Make sure you selected directory with Timberborn exe file.";
    private readonly TimberbornPathPersistence _timberbornPathPersistence = new();

    public bool TryGetTimberbornLibrariesDirectories(out DirectoryInfo librariesDirectory,
                                                     out DirectoryInfo streamingAssetsDirectory) {
      return Application.platform == RuntimePlatform.OSXEditor
          ? TryGetTimberbornLibrariesOnMac(out librariesDirectory, out streamingAssetsDirectory)
          : TryGetTimberbornLibrariesOnWindows(out librariesDirectory,
                                               out streamingAssetsDirectory);
    }

    private bool TryGetTimberbornLibrariesOnMac(out DirectoryInfo librariesDirectory,
                                                out DirectoryInfo streamingAssetsDirectory) {
      var buildPath = EditorUtility.OpenFilePanel("Open Timberborn app",
                                                  GetSavedBuildPath(), "app");
      _timberbornPathPersistence.SavePath(buildPath);
      librariesDirectory = new(Path.Combine(buildPath, "Contents", "Resources", "Data", "Managed"));
      streamingAssetsDirectory =
          new(Path.Combine(buildPath, "Contents", "Resources", "Data", "StreamingAssets"));
      if (librariesDirectory.Exists) {
        return true;
      }
      if (!string.IsNullOrEmpty(buildPath) && ShowTryAgainDialog(MacNotFoundMessage)) {
        return TryGetTimberbornLibrariesOnMac(out librariesDirectory, out streamingAssetsDirectory);
      }
      return false;
    }

    private bool TryGetTimberbornLibrariesOnWindows(out DirectoryInfo librariesDirectory,
                                                    out DirectoryInfo streamingAssetsDirectory) {
      var buildPath = EditorUtility.OpenFolderPanel("Open Timberborn directory",
                                                    GetSavedBuildPath(), "");
      _timberbornPathPersistence.SavePath(buildPath);
      librariesDirectory = new(Path.Combine(buildPath, "Timberborn_Data", "Managed"));
      streamingAssetsDirectory = new(Path.Combine(buildPath, "Timberborn_Data", "StreamingAssets"));
      if (librariesDirectory.Exists) {
        return true;
      }
      if (!string.IsNullOrEmpty(buildPath) && ShowTryAgainDialog(WindowsNotFoundMessage)) {
        return TryGetTimberbornLibrariesOnWindows(out librariesDirectory,
                                                  out streamingAssetsDirectory);
      }
      return false;
    }

    private string GetSavedBuildPath() {
      return _timberbornPathPersistence.TryGetPath(out var path) ? path : string.Empty;
    }

    private static bool ShowTryAgainDialog(string message) {
      return EditorUtility.DisplayDialog("Timberborn build not found",
                                         message + "\nDo you want to try again?", "Yes", "No");
    }

  }
}