using Bindito.Core;
using Timberborn.BottomBarSystem;

namespace Mods.HelloWorld.Scripts {
  [Context("Game")]
  public class HelloWorldConfigurator : Configurator {

    protected override void Configure() {
      Bind<HelloWorldInitializer>().AsSingleton();
      Bind<HelloWorldButton>().AsSingleton();
      Bind<HelloWorldTool>().AsSingleton();

      MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider>().AsSingleton();
    }

    private class BottomBarModuleProvider : IProvider<BottomBarModule> {

      private readonly HelloWorldButton _helloWorldButton;

      public BottomBarModuleProvider(HelloWorldButton helloWorldButton) {
        _helloWorldButton = helloWorldButton;
      }

      public BottomBarModule Get() {
        var builder = new BottomBarModule.Builder();
        builder.AddLeftSectionElement(_helloWorldButton, 200);
        return builder.Build();
      }

    }

  }
}