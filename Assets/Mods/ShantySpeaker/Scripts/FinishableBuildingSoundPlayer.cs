using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.ConstructibleSystem;
using Timberborn.CoreSound;
using Timberborn.SoundSystem;
using UnityEngine;

namespace Mods.ShantySpeaker.Scripts {
  internal class FinishableBuildingSoundPlayer : BaseComponent,
                                                 IFinishedStateListener {

    [SerializeField]
    private string _soundName;

    private ISoundSystem _soundSystem;

    [Inject]
    public void InjectDependencies(ISoundSystem soundSystem) {
      _soundSystem = soundSystem;
    }

    public void OnEnterFinishedState() {
      _soundSystem.LoopSingle3DSound(GameObjectFast, _soundName, 128);
      _soundSystem.SetCustomMixer(GameObjectFast, _soundName, MixerNames.BuildingMixerNameKey);
    }

    public void OnExitFinishedState() {
      _soundSystem.StopSound(GameObjectFast, _soundName);
    }

  }
}