using System.IO;
using System.Linq;
using Timberborn.ModdingTools.Common;

namespace Timberborn.ModdingTools.ModBuilding {
  public class AssetsCopier {

    public static readonly string DataDirectory = "Data";
    private static readonly string RootDirectory = "Root";
    private static readonly string ManifestFileName = "manifest.json";

    public void CopyManifest(ModDefinition modDefinition, DirectoryInfo modDirectory) {
      var manifestPath = Path.Combine(modDefinition.AbsolutePath, ManifestFileName);
      File.Copy(manifestPath, Path.Combine(modDirectory.FullName, ManifestFileName), true);
    }

    public void CopyFiles(ModDefinition modDefinition, DirectoryInfo modDirectory,
                          DirectoryInfo rootDirectory) {
      CopyFiles(modDefinition, DataDirectory, modDirectory);
      CopyFiles(modDefinition, RootDirectory, rootDirectory);
    }

    private void CopyFiles(ModDefinition modDefinition, string sourceDirectoryName,
                           DirectoryInfo targetDirectory) {
      var sourceDirectoryPath = Path.Combine(modDefinition.AbsolutePath, sourceDirectoryName);
      var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);
      if (sourceDirectory.Exists) {
        CreateDirectories(sourceDirectory, targetDirectory);
        CopyFiles(sourceDirectory, targetDirectory);
      }
    }

    private static void CreateDirectories(DirectoryInfo sourceDirectory,
                                          DirectoryInfo targetDirectory) {
      foreach (var subDirectory in sourceDirectory.EnumerateDirectories(
                   "*", SearchOption.AllDirectories)) {
        var relativeDirectoryPath = Path.GetRelativePath(sourceDirectory.FullName,
                                                         subDirectory.FullName);
        Directory.CreateDirectory(Path.Combine(targetDirectory.FullName, relativeDirectoryPath));
      }
    }

    private static void CopyFiles(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory) {
      foreach (var dataFile in sourceDirectory
                   .EnumerateFiles("*", SearchOption.AllDirectories)
                   .Where(file => file.Extension != ".meta")) {
        var relativeFilePath = Path.GetRelativePath(sourceDirectory.FullName,
                                                    dataFile.FullName);
        dataFile.CopyTo(Path.Combine(targetDirectory.FullName, relativeFilePath), true);
      }
    }

  }
}