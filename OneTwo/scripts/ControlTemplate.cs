using Godot;

namespace Main
{

    public class ControlTemplate : Control
    {
        [Export]
        protected bool _active = false;
        public bool Active { get { return _active; } set { _active = value; } }

        [Export]
        private string _buttonGroupName = null;

        protected Godot.Collections.Array<TextureButton> _localButtons;

        public override void _Ready()
        {
            RectPivotOffset = RectSize / 2;

            if (_buttonGroupName != null)
            {
                _localButtons = new Godot.Collections.Array<TextureButton>(GetTree().GetNodesInGroup(_buttonGroupName));
                Main mainNode = (Main)GetTree().GetNodesInGroup("Main")[0];
                Color buttonsColor = new Color(Globals.ColorManager.CurrentColorPalette.ButtonColor, 1f);

                foreach (TextureButton button in _localButtons)
                {
                    button.Connect("pressed", mainNode, $"_on_{Name}_button_pressed", new Godot.Collections.Array { button.Name });
                    button.SelfModulate = buttonsColor;
                }
            }


        }

        public void UpdateState()
        {
            _active = !_active;
            
            Visible = _active;
            DisableButtonsState(!_active);
        }

        public void MoveControl(int index)
        {
            GetParent().MoveChild(this, index);
        }

        public virtual void DisableButtonsState(bool disabled)
        {
            foreach (TextureButton button in _localButtons)
            {
                button.Disabled = disabled;
            }
        }
    }
}