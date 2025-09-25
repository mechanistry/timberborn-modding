using Timberborn.QuickNotificationSystem;
using Timberborn.ToolSystem;

namespace Mods.HelloWorld.Scripts {
  internal class HelloWorldTool : ITool {
    
    private readonly QuickNotificationService _quickNotificationService;

    public HelloWorldTool(QuickNotificationService quickNotificationService) {
      _quickNotificationService = quickNotificationService;
    }

    public void Enter() {
      _quickNotificationService.SendNotification("Hello World, from a tool!");
    }

    public void Exit() {
      _quickNotificationService.SendNotification("Goodbye.");
    }

  }
}