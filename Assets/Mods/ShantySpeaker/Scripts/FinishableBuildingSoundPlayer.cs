using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreSound;
using Timberborn.SoundSystem;

namespace Mods.ShantySpeaker.Scripts {
  internal class FinishableBuildingSoundPlayer : BaseComponent,
                                                 IFinishedStateListener {

    private ISoundSystem _soundSystem;
    private FinishableBuildingSoundPlayerSpec _spec;

    [Inject]
    public void InjectDependencies(ISoundSystem soundSystem) {
      _soundSystem = soundSystem;
    }

    public void Awake() {
      _spec = GetComponentFast<FinishableBuildingSoundPlayerSpec>();
    }

    public void OnEnterFinishedState() {
      _soundSystem.LoopSingle3DSound(GameObjectFast, SoundName, 128);
      _soundSystem.SetCustomMixer(GameObjectFast, SoundName, MixerNames.BuildingMixerNameKey);
    }

    public void OnExitFinishedState() {
      _soundSystem.StopSound(GameObjectFast, SoundName);
    }

    private string SoundName => _spec.SoundName;

  }
}