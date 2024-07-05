using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ModBuilding.Editor {
  internal class AssetBundleBuilder {

    private static readonly string AssetBundlesDirectory = "AssetBundles";
    private static readonly string WinAssetBundleSuffix = "_win";
    private static readonly string MacAssetBundleSuffix = "_mac";
    private static readonly string[] InvalidAssetBundleExtensions = { ".unity", ".cs", ".meta" };

    public void Build(ModDefinition modDefinition, DirectoryInfo modDirectory,
                      ModBuilderSettings modBuilderSettings) {
      var assetBundlesDirectoryPath = Path.Combine(modDefinition.AbsolutePath,
                                                   AssetBundlesDirectory);
      var assetBundlesDirectory = new DirectoryInfo(assetBundlesDirectoryPath);
      if (assetBundlesDirectory.Exists) {
        BuildInternal(modDefinition, modDirectory, assetBundlesDirectory, modBuilderSettings);
      }
    }

    private static void BuildInternal(ModDefinition modDefinition, DirectoryInfo modDirectory,
                                      DirectoryInfo assetBundlesDirectory,
                                      ModBuilderSettings modBuilderSettings) {
      var projectPath = Path.GetDirectoryName(Application.dataPath);
      var assetsToBundle = Directory
          .EnumerateFiles(assetBundlesDirectory.FullName, "*", SearchOption.AllDirectories)
          .Where(path => !InvalidAssetBundleExtensions.Contains(Path.GetExtension(path)))
          .Select(path => Path.GetRelativePath(projectPath, path))
          .ToArray();

      var destinationPath = Path.Combine(modDirectory.FullName, AssetBundlesDirectory);
      Directory.CreateDirectory(destinationPath);
      if (modBuilderSettings.BuildWindowsAssetBundle) {
        BuildWinAssetBundle(modDefinition, assetsToBundle, destinationPath);
      }
      if (modBuilderSettings.BuildMacAssetBundle) {
        BuildMacAssetBundle(modDefinition, assetsToBundle, destinationPath);
      }
      File.Delete(Path.Combine(destinationPath, AssetBundlesDirectory));
      File.Delete(Path.Combine(destinationPath, AssetBundlesDirectory + ".manifest"));
    }

    private static void BuildWinAssetBundle(ModDefinition modDefinition, string[] assets,
                                            string destination) {
      var assetBundle = new AssetBundleBuild {
          assetBundleName = modDefinition.Name + WinAssetBundleSuffix,
          assetNames = assets
      };
      BuildPipeline.BuildAssetBundles(destination, new[] { assetBundle },
                                      BuildAssetBundleOptions.None,
                                      BuildTarget.StandaloneWindows64);
    }

    private static void BuildMacAssetBundle(ModDefinition modDefinition, string[] assets,
                                            string destination) {
      var assetBundle = new AssetBundleBuild {
          assetBundleName = modDefinition.Name + MacAssetBundleSuffix,
          assetNames = assets
      };
      BuildPipeline.BuildAssetBundles(destination, new[] { assetBundle },
                                      BuildAssetBundleOptions.None,
                                      BuildTarget.StandaloneOSX);
    }

  }
}