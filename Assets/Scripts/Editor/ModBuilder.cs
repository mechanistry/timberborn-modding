using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ModBuilding.Editor {
  internal class ModBuilder {

    private static readonly string BuildDirectory = "ModsBuild";
    private static readonly string BuildName = "TimberbornModExamples";
    private static readonly string GameModsDirectory = "Mods";
    private static readonly string UserDataFolder =
        Application.platform == RuntimePlatform.OSXEditor
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                           "Documents",
                           "Timberborn")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                           "Timberborn");
    private readonly List<ModDefinition> _modDefinitions;
    private readonly AssetBundleBuilder _assetBundleBuilder = new();
    private readonly AssetsCopier _assetsCopier = new();
    private readonly DllFilesCopier _dllFilesCopier = new();

    public ModBuilder(IEnumerable<ModDefinition> modDefinitions) {
      _modDefinitions = modDefinitions.ToList();
    }

    public void Build() {
      var buildPath = Path.Combine(UserDataFolder, BuildDirectory, BuildName);
      if (TryBuildProject(buildPath)) {
        foreach (var modDefinition in _modDefinitions) {
          BuildMod(modDefinition, buildPath);
        }
      }
    }

    private static bool TryBuildProject(string buildPath) {
      var buildOptions = new BuildPlayerOptions {
          target = BuildTarget.StandaloneWindows64,
          locationPathName = buildPath,
          scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray()
      };
      return BuildPipeline.BuildPlayer(buildOptions).summary.result == BuildResult.Succeeded;
    }

    private void BuildMod(ModDefinition modDefinition, string buildPath) {
      var directoryPath = Path.Combine(UserDataFolder, GameModsDirectory, modDefinition.Name);
      if (Directory.Exists(directoryPath)) {
        Directory.Delete(directoryPath, true);
      }
      var modDirectory = Directory.CreateDirectory(directoryPath);
      _assetsCopier.CopyManifest(modDefinition, modDirectory);
      _assetsCopier.CopyDataFiles(modDefinition, modDirectory);
      _dllFilesCopier.CopyBuiltDllFiles(modDefinition, modDirectory, buildPath);
      _assetBundleBuilder.Build(modDefinition, modDirectory);
    }

  }
}