using System;
using System.IO;
using UnityEngine;

namespace Timberborn.ModdingTools.Common {
  public static class ModDirectories {

    public static readonly string BuildDirectory = Path.Combine(UserDataFolder, "ModsBuild");
    public static readonly string ModsDirectory = Path.Combine(UserDataFolder, "Mods");

    private static string UserDataFolder =>
        Application.platform == RuntimePlatform.OSXEditor
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                           "Documents",
                           "Timberborn")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                           "Timberborn");

  }
}