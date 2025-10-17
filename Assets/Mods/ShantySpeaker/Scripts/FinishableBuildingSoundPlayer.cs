using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreSound;
using Timberborn.SoundSystem;

namespace Mods.ShantySpeaker.Scripts {
  internal class FinishableBuildingSoundPlayer : BaseComponent,
                                                 IAwakableComponent,
                                                 IFinishedStateListener {

    private ISoundSystem _soundSystem;
    private FinishableBuildingSoundPlayerSpec _spec;

    [Inject]
    public void InjectDependencies(ISoundSystem soundSystem) {
      _soundSystem = soundSystem;
    }

    public void Awake() {
      _spec = GetComponent<FinishableBuildingSoundPlayerSpec>();
    }

    public void OnEnterFinishedState() {
      _soundSystem.LoopSingle3DSound(GameObject, SoundName, 128);
      _soundSystem.SetCustomMixer(GameObject, SoundName, MixerNames.BuildingMixerNameKey);
    }

    public void OnExitFinishedState() {
      _soundSystem.StopSound(GameObject, SoundName);
    }

    private string SoundName => _spec.SoundName;

  }
}