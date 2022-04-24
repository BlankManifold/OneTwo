using Godot;

namespace Main
{

    public class ControlTemplate : Control
    {   
        [Export]
        private bool _active = false;
        public bool Active { get { return _active;} set { _active = value; } }

        [Export]
        private string _buttonGroupName = null;

        public override void _Ready()
        {
            RectPivotOffset = RectSize / 2;

            if (_buttonGroupName != null)
            {
                Godot.Collections.Array<TextureButton> buttons = new Godot.Collections.Array<TextureButton>(GetTree().GetNodesInGroup(_buttonGroupName));
                Main mainNode = (Main)GetTree().GetNodesInGroup("Main")[0];

                foreach (TextureButton button in buttons)
                {
                    button.Connect("pressed", mainNode, $"_on_{Name}_button_pressed", new Godot.Collections.Array { button.Name });
                }
            }
            

        }

        public void UpdateState()
        {
            _active = !_active;
            Visible = _active;
        }

        public void MoveControl(int index)
        {
            GetParent().MoveChild(this, index);
        }
    }
}