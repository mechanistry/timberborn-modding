using Bindito.Core;
using Timberborn.BottomBarSystem;
using Timberborn.EntityPanelSystem;

namespace Mods.HelloWorld.Scripts {
  [Context("Game")]
  public class HelloWorldConfigurator : Configurator {

    protected override void Configure() {
      Bind<HelloWorldInitializer>().AsSingleton();
      Bind<HelloWorldButton>().AsSingleton();
      Bind<HelloWorldTool>().AsSingleton();
      Bind<HelloWorldFragment>().AsSingleton();

      MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider>().AsSingleton();
      MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
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

    private class EntityPanelModuleProvider : IProvider<EntityPanelModule> {

      private readonly HelloWorldFragment _helloWorldFragment;

      public EntityPanelModuleProvider(HelloWorldFragment helloWorldFragment) {
        _helloWorldFragment = helloWorldFragment;
      }

      public EntityPanelModule Get() {
        var builder = new EntityPanelModule.Builder();
        builder.AddTopFragment(_helloWorldFragment);
        return builder.Build();
      }

    }

  }
}