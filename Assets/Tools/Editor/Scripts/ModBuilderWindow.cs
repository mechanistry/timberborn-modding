﻿using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Timberborn.ModdingTools {
  internal class ModBuilderWindow : EditorWindow {

    private static readonly string ModsDirectory = "Assets/Mods";
    private static readonly string ModManifest = "manifest.json";
    private readonly ModBuilderControlsPersistence _modBuilderControlsPersistence = new();
    private readonly GameAutostarter _gameAutostarter = new();
    private readonly ModDirectoryOpener _modDirectoryOpener = new();
    private ScrollView _modList;
    private Label _noModsLabel;
    private Toggle _buildCode;
    private Toggle _buildWindowsAssetBundle;
    private Toggle _buildMacAssetBundle;
    private Toggle _buildZipArchive;
    private TextField _compatibilityVersion;

    [MenuItem("Timberborn/Show Mod Builder", false, 0)]
    public static void ShowWindow() {
      var window = GetWindow<ModBuilderWindow>();
      window.titleContent = new("Mod Builder");
    }

    public void CreateGUI() {
      var rootTree = Resources.Load<VisualTreeAsset>("UI/Views/Editor/ModBuilder/ModBuilderWindow");
      rootTree.CloneTree(rootVisualElement);

      rootVisualElement.Q<Button>("Refresh").RegisterCallback<ClickEvent>(_ => RefreshMods());
      rootVisualElement.Q<Button>("SelectAll")
          .RegisterCallback<ClickEvent>(_ => SetModsEnabledState(true));
      rootVisualElement.Q<Button>("DeselectAll")
          .RegisterCallback<ClickEvent>(_ => SetModsEnabledState(false));

      _modList = rootVisualElement.Q<ScrollView>("ModList");
      _noModsLabel = rootVisualElement.Q<Label>("NoModsLabel");

      _compatibilityVersion = rootVisualElement.Q<TextField>("CompatibilityVersion");
      _buildCode = rootVisualElement.Q<Toggle>("CodeToggle");
      _buildWindowsAssetBundle = rootVisualElement.Q<Toggle>("WindowsAssetBundleToggle");
      _buildMacAssetBundle = rootVisualElement.Q<Toggle>("MacAssetBundleToggle");
      _buildZipArchive = rootVisualElement.Q<Toggle>("ZipArchiveToggle");
      var zipWarningLabel = rootVisualElement.Q<Label>("ZipWarningLabel");
      _buildZipArchive.RegisterValueChangedCallback(
          evt => ToggleDisplayStyle(zipWarningLabel, evt.newValue));
      ToggleDisplayStyle(zipWarningLabel, _buildZipArchive.value);
      rootVisualElement.Q<Button>("DevBuildButton")
          .RegisterCallback<ClickEvent>(_ => RunDevBuild());
      rootVisualElement.Q<Button>("CleanBuildButton")
          .RegisterCallback<ClickEvent>(_ => RunCleanBuild());

      _modBuilderControlsPersistence.InitializeBuildControls(
          _buildCode, _buildWindowsAssetBundle, _buildMacAssetBundle,
          _buildZipArchive, _compatibilityVersion);
      _gameAutostarter.Initialize(rootVisualElement);
      _modDirectoryOpener.Initialize(rootVisualElement);
      RefreshMods();
    }

    private static void ToggleDisplayStyle(VisualElement visualElement, bool visible) {
      visualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void SetModsEnabledState(bool enabled) {
      foreach (var mod in GetMods(false)) {
        _modBuilderControlsPersistence.SetModEnabled(mod, enabled);
      }
      RefreshMods();
    }

    private void RefreshMods() {
      _modList.Clear();
      foreach (var mod in GetMods(false)) {
        _modList.Add(AddModItem(mod));
      }
      ToggleDisplayStyle(_noModsLabel, _modList.childCount == 0);
    }

    private IEnumerable<ModDefinition> GetMods(bool enabledOnly) {
      var projectPath = Path.GetDirectoryName(Application.dataPath);
      foreach (var modFolder in AssetDatabase.GetSubFolders(ModsDirectory)) {
        var manifestPath = $"{modFolder}/{ModManifest}";
        var manifestAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(manifestPath);
        if (manifestAsset) {
          var modDefinition = new ModDefinition(modFolder.Replace(ModsDirectory + "/", ""),
                                                modFolder, $"{projectPath}/{modFolder}");
          if (!enabledOnly || _modBuilderControlsPersistence.IsModEnabled(modDefinition)) {
            yield return modDefinition;
          }
        }
      }
    }

    private VisualElement AddModItem(ModDefinition modDefinition) {
      var toggle = new Toggle {
          value = _modBuilderControlsPersistence.IsModEnabled(modDefinition),
          text = modDefinition.Name
      };
      toggle.RegisterValueChangedCallback(
          evt => _modBuilderControlsPersistence.SetModEnabled(modDefinition, evt.newValue));
      return toggle;
    }

    private void RunDevBuild() {
      var modBuilderSettings = new ModBuilderSettings(_buildCode.value,
                                                      _buildWindowsAssetBundle.value,
                                                      _buildMacAssetBundle.value,
                                                      false,
                                                      false,
                                                      _compatibilityVersion.text);
      RunBuild(modBuilderSettings);
    }

    private void RunCleanBuild() {
      var modBuilderSettings = new ModBuilderSettings(true, true, true, true,
                                                      _buildZipArchive.value,
                                                      _compatibilityVersion.text);
      RunBuild(modBuilderSettings);
    }

    private void RunBuild(ModBuilderSettings modBuilderSettings) {
      var result = new ModBuilder(GetMods(true), modBuilderSettings).Build();
      if (result) {
        Debug.Log("Build completed successfully");
        _gameAutostarter.StartGameIfEnabled();
        _modDirectoryOpener.OpenDirectoryIfEnabled();
      }
    }

  }
}