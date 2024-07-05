using System.Diagnostics;
using System.IO;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace ModBuilding.Editor {
  internal class GameAutostarter {

    private readonly ModBuilderControlsPersistence _modBuilderControlsPersistence = new();
    private Toggle _autostartToggle;
    private TextField _gamePath;
    private TextField _settlementName;
    private TextField _saveName;

    public void Initialize(VisualElement rootVisualElement) {
      _autostartToggle = rootVisualElement.Q<Toggle>("AutostartToggle");
      var autostartValues = rootVisualElement.Q<VisualElement>("AutostartValues");
      _autostartToggle.RegisterValueChangedCallback(
          evt => ToggleDisplayStyle(autostartValues, evt.newValue));

      _gamePath = rootVisualElement.Q<TextField>("GamePath");
      _settlementName = rootVisualElement.Q<TextField>("SettlementName");
      _saveName = rootVisualElement.Q<TextField>("SaveName");
      _modBuilderControlsPersistence.InitializeAutostartControls(_autostartToggle, _gamePath,
                                                                 _settlementName, _saveName);
      ToggleDisplayStyle(autostartValues, _autostartToggle.value);
    }

    public void StartGameIfEnabled() {
      if (_autostartToggle.value) {
        var gamePath = _gamePath.value;
        if (new FileInfo(gamePath).Exists) {
          var processStartInfo = new ProcessStartInfo(gamePath) {
              WindowStyle = ProcessWindowStyle.Normal,
              Arguments = GetStartArguments()
          };
          Process.Start(processStartInfo);
        } else {
          Debug.Log($"Game path does not exist: {gamePath}");
        }
      }
    }

    private static void ToggleDisplayStyle(VisualElement visualElement, bool visible) {
      visualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private string GetStartArguments() {
      var settlementName = _settlementName.value;
      var saveName = _saveName.value;
      if (string.IsNullOrEmpty(settlementName) || string.IsNullOrEmpty(saveName)) {
        return string.Empty;
      }

      return $"-settlementName \"{settlementName}\" -saveName \"{saveName}\"";
    }

  }
}