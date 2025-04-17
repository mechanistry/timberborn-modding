using Timberborn.BaseComponentSystem;
using UnityEngine;

namespace Mods.ShantySpeaker.Scripts {
  internal class FinishableBuildingSoundPlayerSpec : BaseComponent {

    [SerializeField]
    private string _soundName;

    public string SoundName => _soundName;

  }
}