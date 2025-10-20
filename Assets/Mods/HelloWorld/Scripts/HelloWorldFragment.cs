using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.NaturalResources;
using UnityEngine.UIElements;

namespace Mods.HelloWorld.Scripts {
  internal class HelloWorldFragment : IEntityPanelFragment {

    private static readonly string SubPanelClass = "entity-sub-panel";
    private static readonly string SubBoxClass = "bg-sub-box--green";
    private static readonly string ButtonClass = "entity-fragment__button";
    private static readonly string ButtonRedClass = "entity-fragment__button--red";
    private static readonly string DialogBoxLocKey = "HelloWorld.DialogBoxMessage";

    private readonly DialogBoxShower _dialogBoxShower;
    private VisualElement _root;

    public HelloWorldFragment(DialogBoxShower dialogBoxShower) {
      _dialogBoxShower = dialogBoxShower;
    }

    public VisualElement InitializeFragment() {
      _root = new NineSliceVisualElement();
      _root.AddToClassList(SubPanelClass);
      _root.AddToClassList(SubBoxClass);
      _root.style.alignItems = Align.Center;
      _root.ToggleDisplayStyle(false);

      var button = new NineSliceButton {
          text = "Hello World"
      };
      button.AddToClassList(ButtonClass);
      button.AddToClassList(ButtonRedClass);
      button.style.color = UnityEngine.Color.white;
      button.RegisterCallback<ClickEvent>(_ => ShowDialogBox());
      _root.Add(button);

      return _root;
    }

    public void ShowFragment(BaseComponent entity) {
      if (entity.GetComponent<NaturalResource>() is not null) {
        _root.ToggleDisplayStyle(true);
      }
    }

    public void ClearFragment() {
      _root.ToggleDisplayStyle(false);
    }

    public void UpdateFragment() {
    }

    private void ShowDialogBox() {
      _dialogBoxShower.Create()
          .SetLocalizedMessage(DialogBoxLocKey)
          .Show();
    }

  }
}