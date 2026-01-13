using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools.AssetsImporting {
  internal class TimberbornAssetsImporter : EditorWindow {

    public static readonly string DllPath =
        Path.Combine(Application.dataPath, "Plugins", "Timberborn");
    public static readonly string ImportedAssetsPath =
        Path.Combine(Application.dataPath, "Tools", "ImportedAssets");
    private static readonly string ResourcesPath =
        Path.Combine(ImportedAssetsPath, "Editor", "Resources");
    private static readonly string ShadersKey = "Shaders";
    private static readonly string BlueprintsKey = "Blueprints";
    private static readonly string LocalizationsKey = "Localizations";
    private static readonly string UIKey = "UI";
    private static readonly string EditorDllKey = "EditorDll";
    private static readonly string EditorUIKey = "EditorUI";
    private static readonly List<string> ExcludedDllPrefixes = new()
        { "Mono.", "mscorlib.", "netstandard", "System.", "Unity.", "UnityEngine." };
    private static readonly List<string> AlwaysImportedDlls = new() {
        "System.Collections.Immutable.dll",
        "System.Runtime.CompilerServices.Unsafe.dll",
        "System.Threading.Tasks.Extensions.dll",
        "System.Diagnostics.EventLog.dll",
        "System.Text.Encodings.Web.dll",
        "System.Text.Json.dll"
    };
    private static readonly string PublicizeDllPrefix = "Timberborn.";

    [MenuItem("Timberborn/Import Timberborn DLLs and assets", false, 0)]
    public static void Import() {
      var timberbornDirectoryFinder = new TimberbornDirectoryFinder();
      if (timberbornDirectoryFinder.TryGetDirectories(out var dllDirectory,
                                                      out var streamingAssetsDirectory)) {
        ImportDLLs(dllDirectory);
        if (streamingAssetsDirectory.Exists) {
          ImportAssets(streamingAssetsDirectory);
        }
        AssetDatabase.Refresh();
      }
    }

    private static void ImportDLLs(DirectoryInfo dllDirectory) {
      RecreateDirectory(DllPath);
      var dllsCount = 0;
      foreach (var file in dllDirectory.GetFiles()) {
        if (ShouldBeImported(file)) {
          ImportDll(DllPath, file);
          dllsCount++;
        }
      }
      Debug.Log($"Timberborn DLLs ({dllsCount} files) imported successfully.");
    }

    private static bool ShouldBeImported(FileInfo fileInfo) {
      if (AlwaysImportedDlls.Contains(fileInfo.Name)) {
        return true;
      }
      foreach (var prefix in ExcludedDllPrefixes) {
        if (fileInfo.Name.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)) {
          return false;
        }
      }
      return true;
    }

    private static void ImportDll(string destinationPath, FileInfo file) {
      var destination = Path.Combine(destinationPath, file.Name);
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

    private static void ImportAssets(DirectoryInfo streamingAssetsDirectory) {
      RecreateDirectory(ImportedAssetsPath);
      Import(ShadersKey, streamingAssetsDirectory, ShadersKey);
      Import(BlueprintsKey, streamingAssetsDirectory);
      Import(LocalizationsKey, streamingAssetsDirectory, LocalizationsKey);
      Import(UIKey, streamingAssetsDirectory, UIKey, ".txt");
      Import(EditorDllKey, streamingAssetsDirectory, EditorDllKey);
      Import(EditorUIKey, streamingAssetsDirectory, UIKey);
    }

    private static void RecreateDirectory(string path) {
      if (Directory.Exists(path)) {
        Directory.Delete(path, true);
      }
      Directory.CreateDirectory(path);
    }

    private static void Import(string name, DirectoryInfo streamingAssetsDirectory,
                               string destinationDirectory = "", string fileExtension = "") {
      var destinationPath = Path.Combine(ResourcesPath, destinationDirectory);
      var sourcePath = Path.Combine(streamingAssetsDirectory.FullName, "Modding", $"{name}.zip");
      if (File.Exists(sourcePath)) {
        Import(destinationPath, sourcePath, name, fileExtension);
      } else {
        Debug.LogWarning($"Unable to import: {name}. Zip file not found at {sourcePath}");
      }
    }

    private static void Import(string destinationPath, string sourcePath, string name,
                               string fileExtension) {
      using var zipToOpen = new FileStream(sourcePath, FileMode.Open);
      using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);
      foreach (var entry in archive.Entries) {
        ImportEntry(destinationPath, entry, fileExtension);
      }
      Debug.Log($"Timberborn {name} ({archive.Entries.Count} files) imported successfully.");
    }

    private static void ImportEntry(string destinationPath, ZipArchiveEntry entry,
                                    string fileExtension) {
      var fullNameSanitized = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
      var destination = $"{Path.Combine(destinationPath, fullNameSanitized)}{fileExtension}";
      var entryDirectory = Path.GetDirectoryName(destination);
      if (entryDirectory != null && !Directory.Exists(entryDirectory)) {
        Directory.CreateDirectory(entryDirectory);
      }
      entry.ExtractToFile(destination, true);
    }

  }
}