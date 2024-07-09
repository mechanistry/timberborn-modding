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
    private static readonly string WorkshopDataFile = "workshop_data.json";
    private static readonly string UserDataFolder =
        Application.platform == RuntimePlatform.OSXEditor
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                           "Documents",
                           "Timberborn")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                           "Timberborn");
    private readonly List<ModDefinition> _modDefinitions;
    private readonly ModBuilderSettings _modBuilderSettings;
    private readonly AssetBundleBuilder _assetBundleBuilder = new();
    private readonly AssetsCopier _assetsCopier = new();
    private readonly DllFilesCopier _dllFilesCopier = new();

    public ModBuilder(IEnumerable<ModDefinition> modDefinitions,
                      ModBuilderSettings modBuilderSettings) {
      _modDefinitions = modDefinitions.ToList();
      _modBuilderSettings = modBuilderSettings;
    }

    public bool Build() {
      var buildPath = Path.Combine(UserDataFolder, BuildDirectory, BuildName);
      if (!_modBuilderSettings.BuildCode || TryBuildProject(buildPath)) {
        foreach (var modDefinition in _modDefinitions) {
          BuildMod(modDefinition, buildPath);
        }
        return true;
      }
      return false;
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
      var modDirectory = CreateModDirectory(modDefinition);
      _assetsCopier.CopyManifest(modDefinition, modDirectory);
      _assetsCopier.CopyDataFiles(modDefinition, modDirectory);
      if (_modBuilderSettings.BuildCode) {
        _dllFilesCopier.CopyBuiltDllFiles(modDefinition, modDirectory, buildPath);
      }
      if (_modBuilderSettings.BuildWindowsAssetBundle || _modBuilderSettings.BuildMacAssetBundle) {
        _assetBundleBuilder.Build(modDefinition, modDirectory, _modBuilderSettings);
      }
    }

    private DirectoryInfo CreateModDirectory(ModDefinition modDefinition) {
      var modsDirectory = Path.Combine(UserDataFolder, GameModsDirectory);
      var directoryPath = Path.Combine(modsDirectory, modDefinition.Name);
      if (_modBuilderSettings.DeleteFiles && Directory.Exists(directoryPath)) {
        var workshopFilePath = Path.Combine(directoryPath, WorkshopDataFile);
        var workshopData = File.Exists(workshopFilePath)
            ? File.ReadAllText(workshopFilePath)
            : string.Empty;
        Directory.Delete(directoryPath, true);
        var modDirectory = Directory.CreateDirectory(directoryPath);
        if (!string.IsNullOrEmpty(workshopData)) {
          File.WriteAllText(workshopFilePath, workshopData);
        }
        return modDirectory;
      }
      return Directory.CreateDirectory(directoryPath);
    }

  }
}