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
    private VisualElement _warningContainer;
    private VisualTreeAsset _modItemTree;
    private readonly List<ModDefinition> _modDefinitions = new();

    [MenuItem("Timberborn/Show Mod Builder", false, 0)]
    public static void ShowWindow() {
      var window = GetWindow<ModBuilderWindow>();
      window.titleContent = new("Mod Builder");
    }

    public void CreateGUI() {
      var rootAsset = "Assets/Resources/UI/Views/Editor/ModBuilder/ModBuilderWindow.uxml";
      var rootTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(rootAsset);
      rootTree.CloneTree(rootVisualElement);

      var modItemAsset = "Assets/Resources/UI/Views/Editor/ModBuilder/ModItem.uxml";
      _modItemTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(modItemAsset);

      _modList = rootVisualElement.Q<ScrollView>("ModList");
      _noModsLabel = rootVisualElement.Q<Label>("NoModsLabel");
      _warningContainer = rootVisualElement.Q<VisualElement>("WarningContainer");
      rootVisualElement.Q<Button>("BuildSelected")
          .RegisterCallback<ClickEvent>(_ => ShowWarning(GetMods().Where(IsModEnabled)));
      _warningContainer.Q<Button>("Build").RegisterCallback<ClickEvent>(_ => BuildMods());
      _warningContainer.Q<Button>("Cancel").RegisterCallback<ClickEvent>(_ => HideWarning());
      rootVisualElement.Q<Button>("Refresh").RegisterCallback<ClickEvent>(_ => RefreshMods());
      rootVisualElement.Q<Button>("SelectAll")
          .RegisterCallback<ClickEvent>(_ => SetModsEnabledState(true));
      rootVisualElement.Q<Button>("DeselectAll")
          .RegisterCallback<ClickEvent>(_ => SetModsEnabledState(false));
      RefreshMods();
    }

    private void ShowWarning(IEnumerable<ModDefinition> modDefinitions) {
      ToggleDisplayStyle(_warningContainer, true);
      _modDefinitions.Clear();
      _modDefinitions.AddRange(modDefinitions);
    }

    private static void ToggleDisplayStyle(VisualElement visualElement, bool visible) {
      visualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static bool IsModEnabled(ModDefinition modDefinition) {
      return EditorPrefs.GetBool(GetModEnabledKey(modDefinition), true);
    }

    private static void SetModEnabled(ModDefinition modDefinition, bool enabled) {
      EditorPrefs.SetBool(GetModEnabledKey(modDefinition), enabled);
    }

    private static string GetModEnabledKey(ModDefinition modDefinition) {
      return string.Format(ModEnabledKey, modDefinition.Name);
    }

    private void BuildMods() {
      ToggleDisplayStyle(_warningContainer, false);
      new ModBuilder(_modDefinitions).Build();
      _modDefinitions.Clear();
    }

    private void HideWarning() {
      ToggleDisplayStyle(_warningContainer, false);
      _modDefinitions.Clear();
    }

    private void SetModsEnabledState(bool enabled) {
      foreach (var mod in GetMods()) {
        SetModEnabled(mod, enabled);
      }
      RefreshMods();
    }
    
    private void RefreshMods() {
      _modList.Clear();
      foreach (var mod in GetMods()) {
        _modList.Add(AddModItem(mod));
      }
      ToggleDisplayStyle(_noModsLabel, _modList.childCount == 0);
      HideWarning();
    }

    private static IEnumerable<ModDefinition> GetMods() {
      var projectPath = Path.GetDirectoryName(Application.dataPath);
      foreach (var modFolder in AssetDatabase.GetSubFolders(ModsDirectory)) {
        var manifestPath = $"{modFolder}/{ModManifest}";
        var manifestAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(manifestPath);
        if (manifestAsset) {
          yield return new(modFolder.Replace(ModsDirectory + "/", ""), modFolder,
                           $"{projectPath}/{modFolder}");
        }
      }
    }

    private VisualElement AddModItem(ModDefinition modDefinition) {
      var root = new VisualElement();
      _modItemTree.CloneTree(root);
      var toggle = root.Q<Toggle>("BuildToggle");
      toggle.value = IsModEnabled(modDefinition);
      toggle.RegisterValueChangedCallback(evt => SetModEnabled(modDefinition, evt.newValue));
      root.Q<Label>("ModName").text = modDefinition.Name;
      return root;
    }


  }
}