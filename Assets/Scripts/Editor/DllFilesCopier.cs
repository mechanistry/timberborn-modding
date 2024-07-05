using System.IO;
using UnityEditor;

namespace ModBuilding.Editor {
  internal class DllFilesCopier {

    private static readonly string DllDirectory = "_Data/Managed";
    private static readonly string GameModDllDirectory = "Scripts";

    public void CopyBuiltDllFiles(ModDefinition modDefinition, DirectoryInfo modDirectory,
                                  string buildPath) {
      var dllDirectory = buildPath + DllDirectory;
      var modAsmdefs = AssetDatabase.FindAssets("t:asmdef", new[] { modDefinition.ProjectPath });
      foreach (var asmdef in modAsmdefs) {
        var asmdefPath = AssetDatabase.GUIDToAssetPath(asmdef);
        var asmdefName = Path.GetFileNameWithoutExtension(asmdefPath);
        var dllSourcePath = Path.Combine(dllDirectory, $"{asmdefName}.dll");
        var dllDestination = Path.Combine(modDirectory.FullName, GameModDllDirectory);
        Directory.CreateDirectory(dllDestination);
        var dllDestinationPath = Path.Combine(dllDestination, $"{asmdefName}.dll");
        File.Copy(dllSourcePath, dllDestinationPath, true);
      }
    }

  }
}