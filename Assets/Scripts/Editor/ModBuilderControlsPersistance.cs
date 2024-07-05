using UnityEditor;
using UnityEngine.UIElements;

namespace ModBuilding.Editor {
  public class ModBuilderControlsPersistence {

    private static readonly string EditorPrefsKey = "ModBuilderControlsPersistence";
    private static readonly string ModEnabledKey = "ModBuilderWindow.ModEnabled.{0}";

    public void InitializeControls(Toggle buildCode, Toggle buildWindowsAssetBundle,
                                   Toggle buildMacAssetBundle, Toggle autostartToggle,
                                   TextField gamePath, TextField settlementName,
                                   TextField saveName) {
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
      autostartToggle.value = EditorPrefs.GetBool(GetKey(autostartToggle.name), false);
      autostartToggle.RegisterValueChangedCallback(
          evt => EditorPrefs.SetBool(GetKey(autostartToggle.name), evt.newValue));
      gamePath.value = EditorPrefs.GetString(GetKey(gamePath.name), "");
      gamePath.RegisterValueChangedCallback(
          evt => EditorPrefs.SetString(GetKey(gamePath.name), evt.newValue));
      settlementName.value = EditorPrefs.GetString(GetKey(settlementName.name), "");
      settlementName.RegisterValueChangedCallback(
          evt => EditorPrefs.SetString(GetKey(settlementName.name), evt.newValue));
      saveName.value = EditorPrefs.GetString(GetKey(saveName.name), "");
      saveName.RegisterValueChangedCallback(
          evt => EditorPrefs.SetString(GetKey(saveName.name), evt.newValue));
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

  }
}