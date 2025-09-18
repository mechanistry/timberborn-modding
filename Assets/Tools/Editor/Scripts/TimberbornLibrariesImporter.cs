using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Timberborn.ModdingTools {
  internal class TimberbornLibrariesImporter : EditorWindow {

    private static readonly string PluginsDirectory = "Plugins";
    public static readonly string DllDirectory = Path.Combine(PluginsDirectory, "Timberborn");

    private static readonly string ResourcesDirectory = "Resources";
    private static readonly string ModdingDirectory = "Modding";
    private static readonly string ShadersDirectory = Path.Combine(PluginsDirectory, "Shaders");
    private static readonly string LocalizationsDirectory =
        Path.Combine(ResourcesDirectory, "Localizations");
    private static readonly string UIDirectory = Path.Combine(ResourcesDirectory, "UI");

    private static readonly List<string> DllPatterns = new() {
        "Timberborn.*", "AWSSDK.*", "Bindito.*", "Castle.Core.dll",
        "com.rlabrecque.steamworks.net.dll", "LINQtoCSV.dll", "Moq.dll", "protobuf-net*",
        "System.Collections.Immutable.dll", "System.Runtime.CompilerServices.Unsafe.dll",
        "System.Threading.Tasks.Extensions.dll", "System.Diagnostics.EventLog.dll",
        "System.Text.Encodings.Web.dll", "System.Text.Json.dll", "Microsoft.*"
    };
    private static readonly string PublicizeDllPrefix = "Timberborn.";
    private static readonly string ShadersZipSource = Path.Combine(ModdingDirectory, "Shaders.zip");
    private static readonly string BlueprintZipSource =
        Path.Combine(ModdingDirectory, "Blueprints.zip");
    private static readonly string LocalizationsZipSource =
        Path.Combine(ModdingDirectory, "Localizations.zip");
    private static readonly string UIZipSource = Path.Combine(ModdingDirectory, "UI.zip");

    [MenuItem("Timberborn/Import Timberborn DLLs and assets", false, 0)]
    public static void Import() {
      var timberbornLibrariesFinder = new TimberbornLibrariesFinder();
      if (timberbornLibrariesFinder.TryGetTimberbornLibrariesDirectories(
              out var dllDirectory, out var streamingAssetsDirectory)) {
        ImportDLLs(dllDirectory);
        if (streamingAssetsDirectory.Exists) {
          ImportAssets(streamingAssetsDirectory);
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
      Debug.Log($"Timberborn DLLs ({dllsCount} files) imported successfully.");
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

    private static void ImportAssets(DirectoryInfo streamingAssetsDirectory) {
      Import(streamingAssetsDirectory, ShadersDirectory, ShadersZipSource, "Shaders");
      Import(streamingAssetsDirectory, ResourcesDirectory, BlueprintZipSource, "Blueprints", true);
      Import(streamingAssetsDirectory, LocalizationsDirectory, LocalizationsZipSource,
             "Localizations");
      Import(streamingAssetsDirectory, UIDirectory, UIZipSource, "UI");
    }

    private static void Import(DirectoryInfo streamingAssetsDirectory, string destinationDirectory,
                               string zipSource, string name, bool recreateSubdirectories = false) {
      var destinationPath = Path.Combine(Application.dataPath, destinationDirectory);
      var sourcePath = Path.Combine(streamingAssetsDirectory.FullName, zipSource);
      if (File.Exists(sourcePath)) {
        Import(destinationPath, sourcePath, name, recreateSubdirectories);
      } else {
        Debug.LogWarning($"Unable to import: {name}. Zip file not found at {sourcePath}");
      }
    }

    private static void Import(string destinationPath, string sourcePath, string name,
                               bool recreateSubdirectories) {
      using var zipToOpen = new FileStream(sourcePath, FileMode.Open);
      using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);
      if (recreateSubdirectories) {
        RecreateSubDirectories(destinationPath, archive);
      } else {
        RecreateDirectory(destinationPath);
      }
      foreach (var entry in archive.Entries) {
        ImportEntry(destinationPath, entry);
      }
      Debug.Log($"Timberborn {name} ({archive.Entries.Count} files) imported successfully.");
    }

    private static void RecreateSubDirectories(string destinationPath, ZipArchive archive) {
      var directories = archive.Entries
          .Select(entry => entry.FullName.Split(new[] { '/' },
                                                StringSplitOptions.RemoveEmptyEntries))
          .Where(parts => parts.Length > 1)
          .Select(parts => parts[0])
          .Distinct();

      foreach (var directory in directories) {
        RecreateDirectory(Path.Combine(destinationPath, directory));
      }
    }

    private static void ImportEntry(string destinationPath, ZipArchiveEntry entry) {
      var fullNameSanitized = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
      var destination = Path.Combine(destinationPath, fullNameSanitized);
      var entryDirectory = Path.GetDirectoryName(destination);
      if (entryDirectory != null && !Directory.Exists(entryDirectory)) {
        Directory.CreateDirectory(entryDirectory);
      }
      entry.ExtractToFile(destination, true);
    }

  }
}