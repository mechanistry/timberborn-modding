using Bindito.Core;
using Timberborn.TemplateInstantiation;

namespace Mods.ShantySpeaker.Scripts {
  [Context("Game")]
  public class ShantySpeakerConfigurator : Configurator {

    protected override void Configure() {
      Bind<FinishableBuildingSoundPlayer>().AsTransient();

      MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    private static TemplateModule ProvideTemplateModule() {
      var builder = new TemplateModule.Builder();
      builder.AddDecorator<FinishableBuildingSoundPlayerSpec, FinishableBuildingSoundPlayer>();
      return builder.Build();
    }

  }
}