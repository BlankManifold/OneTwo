using System;
using Godot;

namespace Main
{
    public class GameUI : Control
    {
        private Label _movesLabel;

        private Vector2 _bottomPosition;
        public Vector2 BottomPosition { get { return _bottomPosition; }}
        public override void _Ready()
        {
            Main mainNode = (Main)GetTree().GetNodesInGroup("Main")[0];
            Godot.Collections.Array<TextureButton> buttons = new Godot.Collections.Array<TextureButton>(GetTree().GetNodesInGroup("GameUIButton"));

            foreach (TextureButton button in buttons)
            {
                button.Connect("pressed", mainNode, "_on_GameUI_button_pressed", new Godot.Collections.Array { button.Name });
            }

            _movesLabel = GetNode<Label>("MovesTexture/MovesLabel");
            _movesLabel.Text = $"Moves: 0";

            _bottomPosition = GetNode<Position2D>("BottomPosition").GlobalPosition;
            
        }

        public void _on_Grid_UpdateMoves(int moves)
        {
            _movesLabel.Text = $"Moves: {moves}";
        }
    }
}