using System.IO;
using Timberborn.ModdingTools.Common;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools.AssetsImporting {
  internal class TimberbornDirectoryFinder {

    private static readonly string MacNotFoundMessage =
        "File wasn't recognized as Timberborn build. Make sure you selected Timberborn app file.";
    private static readonly string WindowsNotFoundMessage =
        "Directory wasn't recognized as Timberborn build directory. "
        + "Make sure you selected directory with Timberborn exe file.";
    private readonly TimberbornPathPersistence _timberbornPathPersistence = new();

    public bool TryGetDirectories(out DirectoryInfo librariesDirectory,
                                  out DirectoryInfo streamingAssetsDirectory) {
      return Application.platform == RuntimePlatform.OSXEditor
          ? TryGetMacDirectories(out librariesDirectory, out streamingAssetsDirectory)
          : TryGetWindowsDirectories(out librariesDirectory, out streamingAssetsDirectory);
    }

    private bool TryGetMacDirectories(out DirectoryInfo librariesDirectory,
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
        return TryGetMacDirectories(out librariesDirectory, out streamingAssetsDirectory);
      }
      return false;
    }

    private bool TryGetWindowsDirectories(out DirectoryInfo librariesDirectory,
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
        return TryGetWindowsDirectories(out librariesDirectory,
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