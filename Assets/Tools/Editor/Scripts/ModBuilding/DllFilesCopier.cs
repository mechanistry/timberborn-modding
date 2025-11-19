using System.IO;
using Timberborn.ModdingTools.Common;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools.ModBuilding {
  internal class DllFilesCopier {

    private static readonly string WindowsBuildPostfix = "_Data";
    private static readonly string MacBuildPostfix = ".app";
    private static readonly string GameModDllDirectory = "Scripts";

    public void CopyBuiltDllFiles(ModDefinition modDefinition, DirectoryInfo modDirectory,
                                  string buildPath) {
      var dllDirectory = GetDLLDirectory(buildPath);
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

    private static string GetDLLDirectory(string buildPath) {
      if (Application.platform == RuntimePlatform.OSXEditor) {
        return Path.Combine(buildPath + MacBuildPostfix, "Contents", "Resources", "Data",
                            "Managed");
      }
      return Path.Combine(buildPath + WindowsBuildPostfix, "Managed");
    }

  }
}