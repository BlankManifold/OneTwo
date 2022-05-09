using Godot;

namespace Main
{

    public class MainControl : ControlTemplate
    {
        public override void _Ready()
        {
            base._Ready();
        
            _localButtons = new Godot.Collections.Array<TextureButton>(GetTree().GetNodesInGroup("GameUIButton"));
        }

    }
}