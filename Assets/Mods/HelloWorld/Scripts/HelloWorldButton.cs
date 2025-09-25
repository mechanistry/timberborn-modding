using System.Collections.Generic;
using Timberborn.BottomBarSystem;
using Timberborn.ToolButtonSystem;

namespace Mods.HelloWorld.Scripts {
  internal class HelloWorldButton : IBottomBarElementsProvider {

    private static readonly string ToolImageKey = "HelloWorldTool";
    private readonly HelloWorldTool _helloWorldTool;
    private readonly ToolButtonFactory _toolButtonFactory;

    public HelloWorldButton(HelloWorldTool helloWorldTool,
                            ToolButtonFactory toolButtonFactory) {
      _helloWorldTool = helloWorldTool;
      _toolButtonFactory = toolButtonFactory;
    }

    public IEnumerable<BottomBarElement> GetElements() {
      var button = _toolButtonFactory.CreateGrouplessRed(_helloWorldTool, ToolImageKey);
      yield return BottomBarElement.CreateSingleLevel(button.Root);
    }

    

  }
}