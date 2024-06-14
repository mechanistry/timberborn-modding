using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;

namespace Mods.HelloWorld.Scripts {
  public class HelloWorldInitializer : ILoadableSingleton {

    private readonly UILayout _uiLayout;
    private readonly VisualElementLoader _visualElementLoader;

    public HelloWorldInitializer(UILayout uiLayout, 
                                 VisualElementLoader visualElementLoader) {
      _uiLayout = uiLayout;
      _visualElementLoader = visualElementLoader;
    }

    public void Load() {
      var visualElement = _visualElementLoader.LoadVisualElement("HelloWorld");
      _uiLayout.AddBottomRight(visualElement, 0);
    }

  }
}