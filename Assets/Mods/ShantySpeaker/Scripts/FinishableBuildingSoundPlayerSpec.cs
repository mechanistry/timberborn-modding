using Timberborn.BlueprintSystem;

namespace Mods.ShantySpeaker.Scripts {
  internal record FinishableBuildingSoundPlayerSpec : ComponentSpec {

    [Serialize]
    public string SoundName { get; init; }

  }
}