using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ModBuilding.Editor {
  internal class TimberbornLibrariesImporter : EditorWindow {

    public static readonly string PluginsDirectory = Path.Combine("Plugins", "Timberborn");
    private static readonly List<string> DllPatterns = new() {
        "Timberborn.*", "AWSSDK.*", "Bindito.*", "Castle.Core.dll",
        "com.rlabrecque.steamworks.net.dll", "LINQtoCSV.dll", "Moq.dll", "protobuf-net.dll",
        "System.Collections.Immutable.dll", "System.Runtime.CompilerServices.Unsafe.dll",
        "System.Threading.Tasks.Extensions.dll"
    };

    [MenuItem("Timberborn/Import Timberborn dlls", false, 0)]
    public static void Import() {
      var timberbornLibrariesFinder = new TimberbornLibrariesFinder();
      if (timberbornLibrariesFinder.TryGetTimberbornLibrariesDirectory(out var dllDirectory)) {
        Import(dllDirectory);
      }
    }

    private static void Import(DirectoryInfo dllDirectory) {
      var pluginsPath = Path.Combine(Application.dataPath, PluginsDirectory);
      RecreatePluginsDirectory(pluginsPath);
      foreach (var file in dllDirectory.GetFiles()) {
        if (ShouldBeImported(file)) {
          ImportDll(pluginsPath, file);
        }
      }
      AssetDatabase.Refresh();
    }

    private static void RecreatePluginsDirectory(string pluginsPath) {
      if (Directory.Exists(pluginsPath)) {
        Directory.Delete(pluginsPath, true);
      }
      Directory.CreateDirectory(pluginsPath);
    }

    private static void ImportDll(string pluginsPath, FileInfo file) {
      var destination = Path.Combine(pluginsPath, file.Name);
      File.Copy(file.FullName, destination, true);
      using var metaFile = File.CreateText(destination + ".meta");
      metaFile.WriteLine("fileFormatVersion: 2");
      metaFile.WriteLine("guid: " + GenerateGuid(file.Name));
    }

    private static bool ShouldBeImported(FileInfo fileInfo) {
      foreach (var dllPattern in DllPatterns) {
        if (Regex.IsMatch(fileInfo.Name, dllPattern)) {
          return true;
        }
      }
      return false;
    }

    private static string GenerateGuid(string inputString) {
      var guid = new StringBuilder();
      using HashAlgorithm algorithm = SHA256.Create();
      var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
      for (var index = 0; index < 16; index++) {
        guid.Append(index < hash.Length ? hash[index].ToString("X2") : "00");
      }
      return guid.ToString();
    }

  }
}