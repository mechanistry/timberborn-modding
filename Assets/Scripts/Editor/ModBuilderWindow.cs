using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ModBuilding.Editor {
  internal class ModBuilderWindow : EditorWindow {

    private static readonly string ModsDirectory = "Assets/Mods";
    private static readonly string ModManifest = "manifest.json";
    private static readonly string ModEnabledKey = "ModBuilderWindow.ModEnabled.{0}";
    private ScrollView _modList;
    private Label _noModsLabel;
    private Toggle _buildCode;
    private Toggle _buildWindowsAssetBundle;
    private Toggle _buildMacAssetBundle;

    [MenuItem("Timberborn/Show Mod Builder", false, 0)]
    public static void ShowWindow() {
      var window = GetWindow<ModBuilderWindow>();
      window.titleContent = new("Mod Builder");
    }

    public void CreateGUI() {
      var rootAsset = "Assets/Resources/UI/Views/Editor/ModBuilder/ModBuilderWindow.uxml";
      var rootTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(rootAsset);
      rootTree.CloneTree(rootVisualElement);
      
      rootVisualElement.Q<Button>("Refresh").RegisterCallback<ClickEvent>(_ => RefreshMods());
      rootVisualElement.Q<Button>("SelectAll")
          .RegisterCallback<ClickEvent>(_ => SetModsEnabledState(true));
      rootVisualElement.Q<Button>("DeselectAll")
          .RegisterCallback<ClickEvent>(_ => SetModsEnabledState(false));

      _modList = rootVisualElement.Q<ScrollView>("ModList");
      _noModsLabel = rootVisualElement.Q<Label>("NoModsLabel");
      
      _buildCode = rootVisualElement.Q<Toggle>("CodeToggle");
      _buildWindowsAssetBundle = rootVisualElement.Q<Toggle>("WindowsAssetBundleToggle");
      _buildMacAssetBundle = rootVisualElement.Q<Toggle>("MacAssetBundleToggle");
      rootVisualElement.Q<Button>("DevBuildButton")
          .RegisterCallback<ClickEvent>(_ => RunDevBuild());
      rootVisualElement.Q<Button>("CleanBuildButton")
          .RegisterCallback<ClickEvent>(_ => RunCleanBuild());
      
      RefreshMods();
    }

    private static void ToggleDisplayStyle(VisualElement visualElement, bool visible) {
      visualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }


    private static void SetModEnabled(ModDefinition modDefinition, bool enabled) {
      EditorPrefs.SetBool(GetModEnabledKey(modDefinition), enabled);
    }

    private static string GetModEnabledKey(ModDefinition modDefinition) {
      return string.Format(ModEnabledKey, modDefinition.Name);
    }

    private void RunDevBuild() {
      var modBuilderSettings = new ModBuilderSettings(_buildCode.value, 
                                                      _buildWindowsAssetBundle.value,
                                                      _buildMacAssetBundle.value, 
                                                      false);
      new ModBuilder(GetMods(true), modBuilderSettings).Build();
    }
    
    private void RunCleanBuild() {
      var modBuilderSettings = new ModBuilderSettings(true, true, true, true);
      new ModBuilder(GetMods(true), modBuilderSettings).Build();
    }

    private void SetModsEnabledState(bool enabled) {
      foreach (var mod in GetMods(false)) {
        SetModEnabled(mod, enabled);
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

    private static IEnumerable<ModDefinition> GetMods(bool enabledOnly) {
      var projectPath = Path.GetDirectoryName(Application.dataPath);
      foreach (var modFolder in AssetDatabase.GetSubFolders(ModsDirectory)) {
        var manifestPath = $"{modFolder}/{ModManifest}";
        var manifestAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(manifestPath);
        if (manifestAsset) {
          var modDefinition = new ModDefinition(modFolder.Replace(ModsDirectory + "/", ""), 
                                                modFolder, $"{projectPath}/{modFolder}");
          if(!enabledOnly || IsModEnabled(modDefinition)) {
            yield return modDefinition;
          }
        }
      }
    }

    private static bool IsModEnabled(ModDefinition modDefinition) {
      return EditorPrefs.GetBool(GetModEnabledKey(modDefinition), true);
    }
    
    private VisualElement AddModItem(ModDefinition modDefinition) {
      var toggle = new Toggle {
          value = IsModEnabled(modDefinition),
          text = modDefinition.Name
      };
      toggle.RegisterValueChangedCallback(evt => SetModEnabled(modDefinition, evt.newValue));
      return toggle;
    }


  }
}