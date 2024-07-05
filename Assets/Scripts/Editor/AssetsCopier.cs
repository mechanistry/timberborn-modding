using System.IO;
using System.Linq;

namespace ModBuilding.Editor {
  internal class AssetsCopier {

    private static readonly string DataDirectory = "Data";
    private static readonly string ManifestFileName = "manifest.json";

    public void CopyManifest(ModDefinition modDefinition, DirectoryInfo modDirectory) {
      var manifestPath = Path.Combine(modDefinition.AbsolutePath, ManifestFileName);
      File.Copy(manifestPath, Path.Combine(modDirectory.FullName, ManifestFileName), true);
    }

    public void CopyDataFiles(ModDefinition modDefinition, DirectoryInfo modDirectory) {
      var dataFilesDirectoryPath = Path.Combine(modDefinition.AbsolutePath, DataDirectory);
      var dataFilesDirectory = new DirectoryInfo(dataFilesDirectoryPath);
      if (dataFilesDirectory.Exists) {
        CreateDirectories(modDirectory, dataFilesDirectory);
        CopyFiles(modDirectory, dataFilesDirectory);
      }
    }

    private static void CreateDirectories(DirectoryInfo modDirectory,
                                          DirectoryInfo dataFilesDirectory) {
      foreach (var subDirectory in dataFilesDirectory
                   .EnumerateDirectories("*", SearchOption.AllDirectories)) {
        var relativeDirectoryPath = Path.GetRelativePath(dataFilesDirectory.FullName,
                                                         subDirectory.FullName);
        Directory.CreateDirectory(Path.Combine(modDirectory.FullName, relativeDirectoryPath));
      }
    }

    private static void CopyFiles(DirectoryInfo modDirectory, DirectoryInfo dataFilesDirectory) {
      foreach (var dataFile in dataFilesDirectory
                   .EnumerateFiles("*", SearchOption.AllDirectories)
                   .Where(file => file.Extension != ".meta")) {
        var relativeFilePath = Path.GetRelativePath(dataFilesDirectory.FullName,
                                                    dataFile.FullName);
        dataFile.CopyTo(Path.Combine(modDirectory.FullName, relativeFilePath), true);
      }
    }

  }
}