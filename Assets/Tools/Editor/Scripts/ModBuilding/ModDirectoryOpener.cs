using System.Diagnostics;
using Timberborn.ModdingTools.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Timberborn.ModdingTools.ModBuilding {
  internal class ModDirectoryOpener {

    private readonly ModBuilderControlsPersistence _modBuilderControlsPersistence = new();
    private Toggle _browseModsDirectoryToggle;

    public void Initialize(VisualElement root) {
      _browseModsDirectoryToggle = root.Q<Toggle>("BrowseModsDirectoryToggle");
      _modBuilderControlsPersistence.InitializeModsDirectoryControls(_browseModsDirectoryToggle);
      var button = root.Q<Button>("BrowseModsDirectoryButton");
      button.RegisterCallback<ClickEvent>(_ => BrowseModsDirectory());
    }

    public void OpenDirectoryIfEnabled() {
      if (_browseModsDirectoryToggle.value) {
        BrowseModsDirectory();
      }
    }

    private static void BrowseModsDirectory() {
      if (Application.platform == RuntimePlatform.OSXEditor) {
        Process.Start("open", ModDirectories.ModsDirectory);
      } else {
        Application.OpenURL(ModDirectories.ModsDirectory);
      }
    }

  }
}