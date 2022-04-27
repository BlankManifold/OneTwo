using System;
using Godot;

namespace Main
{
    public class GameUI : ControlTemplate
    {
        private Label _movesLabel;

        private Vector2 _bottomPosition;
        public Vector2 BottomPosition { get { return _bottomPosition; }}
        public override void _Ready()
        {
            base._Ready();
            
            _movesLabel = GetNode<Label>("MovesLabel");
            _movesLabel.Text = $"Moves: 0";

            _bottomPosition = GetNode<Position2D>("BottomPosition").GlobalPosition;
            
        }

        public void _on_Grid_UpdateMoves(int moves)
        {
            _movesLabel.Text = $"Moves: {moves}";
        }
    }
}