using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModBuilding.Editor {
  internal class ModBuilderControlsPersistence {

    private static readonly string EditorPrefsKey = "ModBuilderControlsPersistence";
    private static readonly string ModEnabledKey = "ModBuilderWindow.ModEnabled.{0}";
    private readonly TimberbornPathPersistence _timberbornPathPersistence = new();

    public void InitializeBuildControls(Toggle buildCode, Toggle buildWindowsAssetBundle,
                                        Toggle buildMacAssetBundle) {
      buildCode.value = EditorPrefs.GetBool(GetKey(buildCode.name), true);
      buildCode.RegisterValueChangedCallback(
          evt => EditorPrefs.SetBool(GetKey(buildCode.name), evt.newValue));
      buildWindowsAssetBundle.value =
          EditorPrefs.GetBool(GetKey(buildWindowsAssetBundle.name), true);
      buildWindowsAssetBundle.RegisterValueChangedCallback(
          evt => EditorPrefs.SetBool(GetKey(buildWindowsAssetBundle.name), evt.newValue));
      buildMacAssetBundle.value = EditorPrefs.GetBool(GetKey(buildMacAssetBundle.name), true);
      buildMacAssetBundle.RegisterValueChangedCallback(
          evt => EditorPrefs.SetBool(GetKey(buildMacAssetBundle.name), evt.newValue));
    }

    public void InitializeAutostartControls(Toggle autostartToggle, Toggle runOnSteamToggle,
                                            TextField gamePath, TextField settlementName,
                                            TextField saveName, TextField customArguments) {
      autostartToggle.value = EditorPrefs.GetBool(GetKey(autostartToggle.name), false);
      autostartToggle.RegisterValueChangedCallback(
          evt => EditorPrefs.SetBool(GetKey(autostartToggle.name), evt.newValue));
      runOnSteamToggle.value = EditorPrefs.GetBool(GetKey(runOnSteamToggle.name), true);
      runOnSteamToggle.RegisterValueChangedCallback(
          evt => EditorPrefs.SetBool(GetKey(runOnSteamToggle.name), evt.newValue));
      gamePath.value = EditorPrefs.GetString(GetKey(gamePath.name), string.Empty);
      if (string.IsNullOrEmpty(gamePath.value)) {
        gamePath.value = GetSavedExecutablePath();
      }
      gamePath.RegisterValueChangedCallback(
          evt => EditorPrefs.SetString(GetKey(gamePath.name), evt.newValue));
      settlementName.value = EditorPrefs.GetString(GetKey(settlementName.name), string.Empty);
      settlementName.RegisterValueChangedCallback(
          evt => EditorPrefs.SetString(GetKey(settlementName.name), evt.newValue));
      saveName.value = EditorPrefs.GetString(GetKey(saveName.name), string.Empty);
      saveName.RegisterValueChangedCallback(
          evt => EditorPrefs.SetString(GetKey(saveName.name), evt.newValue));
      customArguments.value = EditorPrefs.GetString(GetKey(customArguments.name), string.Empty);
      customArguments.RegisterValueChangedCallback(
          evt => EditorPrefs.SetString(GetKey(customArguments.name), evt.newValue));
    }

    public void SetModEnabled(ModDefinition modDefinition, bool enabled) {
      EditorPrefs.SetBool(GetModEnabledKey(modDefinition), enabled);
    }

    public bool IsModEnabled(ModDefinition modDefinition) {
      return EditorPrefs.GetBool(GetModEnabledKey(modDefinition), true);
    }

    private static string GetModEnabledKey(ModDefinition modDefinition) {
      return string.Format(ModEnabledKey, modDefinition.Name);
    }

    private static string GetKey(string controlName) {
      return $"{EditorPrefsKey}.{controlName}";
    }

    private string GetSavedExecutablePath() {
      if (_timberbornPathPersistence.TryGetPath(out var path)) {
        return Application.platform == RuntimePlatform.OSXEditor
            ? Path.Combine(path, "Contents", "MacOS", "Timberborn")
            : Path.Combine(path, "Timberborn.exe");
      }
      return string.Empty;
    }

  }
}