using Bindito.Core;

namespace Mods.HelloWorld.Scripts {
  [Context("Game")]
  public class HelloWorldConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<HelloWorldInitializer>().AsSingleton();
    }

  }
}