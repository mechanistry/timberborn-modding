using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools {
  internal class TimberbornLibrariesImporter : EditorWindow {

    private static readonly string PluginsDirectory = "Plugins";
    public static readonly string DllDirectory = Path.Combine(PluginsDirectory, "Timberborn");

    private static readonly string ShadersDirectory = Path.Combine(PluginsDirectory, "Shaders");
    private static readonly List<string> DllPatterns = new() {
        "Timberborn.*", "AWSSDK.*", "Bindito.*", "Castle.Core.dll",
        "com.rlabrecque.steamworks.net.dll", "LINQtoCSV.dll", "Moq.dll", "protobuf-net*",
        "System.Collections.Immutable.dll", "System.Runtime.CompilerServices.Unsafe.dll",
        "System.Threading.Tasks.Extensions.dll", "System.Diagnostics.EventLog.dll",
        "System.Text.Encodings.Web.dll", "System.Text.Json.dll",
        "Microsoft.Bcl.AsyncInterfaces.dll"
    };
    private static readonly string PublicizeDllPrefix = "Timberborn.";
    private static readonly string ShadersZipSource = Path.Combine("Modding", "Shaders.zip");

    [MenuItem("Timberborn/Import Timberborn DLLs and shaders", false, 0)]
    public static void Import() {
      var timberbornLibrariesFinder = new TimberbornLibrariesFinder();
      if (timberbornLibrariesFinder.TryGetTimberbornLibrariesDirectories(
              out var dllDirectory, out var streamingAssetsDirectory)) {
        ImportDLLs(dllDirectory);
        if (streamingAssetsDirectory.Exists) {
          ImportShaders(streamingAssetsDirectory);
        }
        AssetDatabase.Refresh();
      }
    }

    private static void ImportDLLs(DirectoryInfo dllDirectory) {
      var dllPath = Path.Combine(Application.dataPath, DllDirectory);
      RecreateDirectory(dllPath);
      var dllsCount = 0;
      foreach (var file in dllDirectory.GetFiles()) {
        if (ShouldBeImported(file)) {
          ImportDll(dllPath, file);
          dllsCount++;
        }
      }
      Debug.Log($"Timberborn DLLs ({dllsCount}) imported successfully.");
    }

    private static void RecreateDirectory(string path) {
      if (Directory.Exists(path)) {
        Directory.Delete(path, true);
      }
      Directory.CreateDirectory(path);
    }

    private static bool ShouldBeImported(FileInfo fileInfo) {
      foreach (var dllPattern in DllPatterns) {
        if (Regex.IsMatch(fileInfo.Name, dllPattern)) {
          return true;
        }
      }
      return false;
    }

    private static void ImportDll(string pluginsPath, FileInfo file) {
      var destination = Path.Combine(pluginsPath, file.Name);
      File.Copy(file.FullName, destination, true);

      if (file.Name.StartsWith(PublicizeDllPrefix)) {
        DllPublicizer.PublicizeDLL(destination, file.Directory);
      }

      using var metaFile = File.CreateText(destination + ".meta");
      metaFile.WriteLine("fileFormatVersion: 2");
      metaFile.WriteLine("guid: " + GenerateGuid(file.Name));
    }

    private static string GenerateGuid(string fileName) {
      using var md5 = MD5.Create();
      var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
      var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(fileNameWithoutExtension));
      return new Guid(hash).ToString().ToLower().Replace("-", "");
    }

    private static void ImportShaders(DirectoryInfo streamingAssetsDirectory) {
      var destinationDirectory = Path.Combine(Application.dataPath, ShadersDirectory);
      RecreateDirectory(destinationDirectory);

      var shadersPath = Path.Combine(streamingAssetsDirectory.FullName, ShadersZipSource);
      using var zipToOpen = new FileStream(shadersPath, FileMode.Open);
      using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);
      foreach (var entry in archive.Entries) {
        var destination = Path.Combine(destinationDirectory, entry.Name);
        entry.ExtractToFile(destination, true);
      }
      var noMetaFilesCount = archive.Entries.Count / 2;
      Debug.Log($"Timberborn Shaders ({noMetaFilesCount}) imported successfully.");
    }

  }
}