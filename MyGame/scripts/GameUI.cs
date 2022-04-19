using System;
using Godot;

namespace Main
{
    public class GameUI : Control
    {
        public override void _Ready()
        {
            Main mainNode = (Main)GetTree().GetNodesInGroup("Main")[0];
            Godot.Collections.Array<TextureButton> buttons = new Godot.Collections.Array<TextureButton>(GetTree().GetNodesInGroup("GameUIButton"));

            foreach (TextureButton button in buttons)
            {
                button.Connect("pressed", mainNode, "_on_GameUI_button_pressed", new Godot.Collections.Array { button.Name });
            }
        }
    }
}