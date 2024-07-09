using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace ModBuilding.Editor {
  internal class GameAutostarter {

    private static readonly string AppId = "1062090";
    private readonly ModBuilderControlsPersistence _modBuilderControlsPersistence = new();
    private Toggle _autostartToggle;
    private Toggle _runOnSteamToggle;
    private TextField _gamePath;
    private TextField _settlementName;
    private TextField _saveName;
    private TextField _customArguments;

    public void Initialize(VisualElement rootVisualElement) {
      _autostartToggle = rootVisualElement.Q<Toggle>("AutostartToggle");
      var autostartValues = rootVisualElement.Q<VisualElement>("AutostartValues");
      _autostartToggle.RegisterValueChangedCallback(
          evt => ToggleDisplayStyle(autostartValues, evt.newValue));

      _runOnSteamToggle = rootVisualElement.Q<Toggle>("RunOnSteamToggle");
      _runOnSteamToggle.RegisterValueChangedCallback(evt => ToggleDisplayStyle(_gamePath,
                                                                               !evt.newValue));
      _gamePath = rootVisualElement.Q<TextField>("GamePath");
      _settlementName = rootVisualElement.Q<TextField>("SettlementName");
      _saveName = rootVisualElement.Q<TextField>("SaveName");
      _customArguments = rootVisualElement.Q<TextField>("CustomArguments");
      _modBuilderControlsPersistence.InitializeAutostartControls(
          _autostartToggle, _runOnSteamToggle, _gamePath,
          _settlementName, _saveName, _customArguments);
      ToggleDisplayStyle(autostartValues, _autostartToggle.value);
      ToggleDisplayStyle(_gamePath, !_runOnSteamToggle.value);
    }

    public void StartGameIfEnabled() {
      if (_autostartToggle.value) {
        if (_runOnSteamToggle.value) {
          RunOnSteam();
        } else {
          RunFromFile();
        }
      }
    }
    
    private static void ToggleDisplayStyle(VisualElement visualElement, bool visible) {
      visualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
    
    private void RunOnSteam() {
      Application.OpenURL($"steam://run/{AppId}//{GetCustomArguments("\\")}/");
    }

    private void RunFromFile() {
      var gamePath = _gamePath.value;
      if (new FileInfo(gamePath).Exists) {
        var processStartInfo = new ProcessStartInfo(gamePath) {
            WindowStyle = ProcessWindowStyle.Normal,
            Arguments = GetCustomArguments(string.Empty)
        };
        Process.Start(processStartInfo);
      } else {
        Debug.Log($"Game path does not exist: {gamePath}");
      }
    }
    
    private string GetCustomArguments(string escapeCharacter) {
      return $"{_customArguments.value}{GetStartArguments(escapeCharacter)}";
    }

    private string GetStartArguments(string escapeCharacter) {
      var settlementName = _settlementName.value;
      var saveName = _saveName.value;
      if (string.IsNullOrEmpty(settlementName) || string.IsNullOrEmpty(saveName)) {
        return string.Empty;
      }

      return $" -settlementName {escapeCharacter}\"{settlementName}{escapeCharacter}\" "
             + $"-saveName {escapeCharacter}\"{saveName}{escapeCharacter}\"";
    }

  }
}