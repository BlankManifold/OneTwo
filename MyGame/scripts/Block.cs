using System;
using Godot;

namespace Main
{
    public class Block : Sprite
    {
        private bool _flipped = false;
        private Tween _tween;
        private Vector2 _cellCoords;
        public Vector2 CellCoords
        {
            get { return _cellCoords; }
        }

        private Color _color = Globals.ColorPalette.DefaultColor;
        private int _colorId = -1;

        public void Init(Vector2 cellCoords, int cellSize, int cellBorder, int colorId = 0)
        {
            _cellCoords = cellCoords;
            _colorId = colorId;

            Position = cellCoords * (cellSize + cellBorder);
            Modulate = Globals.ColorPalette.DefaultColor;
            _color = Globals.ColorPalette.GetColor(_colorId);
        }
        
        public void InitColor(int colorId = 0)
        {
            _colorId = colorId;
            _color = Globals.ColorPalette.GetColor(_colorId);
        }
        public void Restart()
        {
            _flipped = false;
            Modulate = Globals.ColorPalette.DefaultColor;
            _colorId = -1;
        }

        public override void _Ready()
        {
            _tween = GetNode<Tween>("Tween");
        }

        public void Flip()
        {
            _flipped = !_flipped;
            if (_flipped)
            {
                Modulate = _color;
                return;
            }
            
            Modulate = Globals.ColorPalette.DefaultColor;

        }
        public void GoTo(Vector2 position)
        {
            _tween.InterpolateProperty(this,"position", Position, position, 0.2f);
            _tween.Start();
            // Position = position;
        }

    }
}