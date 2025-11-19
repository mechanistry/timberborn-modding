using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Timberborn.ModdingTools.Common;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Timberborn.ModdingTools.ModBuilding {
  internal class ModBuilder {

    private static readonly string BuildName = "TimberbornModExamples";
    private static readonly string WorkshopDataFile = "workshop_data.json";
    private static readonly string CompatibilityVersionPrefix = "version-";
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
      var buildPath = Path.Combine(ModDirectories.BuildDirectory, BuildName);
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
          target = Application.platform == RuntimePlatform.OSXEditor
              ? BuildTarget.StandaloneOSX
              : BuildTarget.StandaloneWindows64,
          locationPathName = buildPath,
          scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray()
      };
      return BuildPipeline.BuildPlayer(buildOptions).summary.result == BuildResult.Succeeded;
    }

    private void BuildMod(ModDefinition modDefinition, string buildPath) {
      var modDirectory = CreateModDirectory(modDefinition, out var rootDirectory);
      _assetsCopier.CopyManifest(modDefinition, modDirectory);
      _assetsCopier.CopyFiles(modDefinition, modDirectory, rootDirectory);
      if (_modBuilderSettings.BuildCode) {
        _dllFilesCopier.CopyBuiltDllFiles(modDefinition, modDirectory, buildPath);
      }
      if (_modBuilderSettings.BuildWindowsAssetBundle || _modBuilderSettings.BuildMacAssetBundle) {
        _assetBundleBuilder.Build(modDefinition, modDirectory, _modBuilderSettings);
      }
      if (_modBuilderSettings.BuildZipArchive) {
        BuildZipArchive(modDefinition, rootDirectory);
      }
    }

    private DirectoryInfo CreateModDirectory(ModDefinition modDefinition,
                                             out DirectoryInfo rootDirectory) {
      var rootDirectoryPath = Path.Combine(ModDirectories.ModsDirectory, modDefinition.Name);
      rootDirectory = Directory.CreateDirectory(rootDirectoryPath);
      var directoryPath = string.IsNullOrEmpty(_modBuilderSettings.CompatibilityVersion)
          ? rootDirectoryPath
          : Path.Combine(rootDirectoryPath,
                         CompatibilityVersionPrefix + _modBuilderSettings.CompatibilityVersion);
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

    private static void BuildZipArchive(ModDefinition modDefinition, DirectoryInfo rootDirectory) {
      var zipFilePath = Path.Combine(ModDirectories.ModsDirectory, modDefinition.Name + ".zip");
      File.Delete(zipFilePath);
      ZipFile.CreateFromDirectory(rootDirectory.FullName, zipFilePath);
    }

  }
}