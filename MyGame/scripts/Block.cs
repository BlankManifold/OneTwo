using System;
using Godot;

namespace Main
{
    public class Block : Sprite
    {
        private bool _flipped = false;
        public bool Flipped { get { return _flipped; } set { _flipped = value; } }

        private bool _isOff = false;
        public bool IsOff { get { return _isOff; } }

        private Vector2 _originalScale;
        public Vector2 OriginalScale { get { return _originalScale; } }
        private Vector2 _originalPosition;
        private Vector2 _originalCoords;


        private Globals.BLOCKSTATE _state = Globals.BLOCKSTATE.IDLE;
        public Globals.BLOCKSTATE State { get { return _state; } set { _state = value; } }



        private Vector2 _cellCoords;
        public Vector2 CellCoords
        {
            get { return _cellCoords; }
            set { _cellCoords = value; }
        }

        private Color _color = Globals.ColorManager.CurrentColorPalette.DefaultColor;
        public Color Color { get { return _color; } }

        private int _colorId = -1;
        public int ColorId { get { return _colorId; } }

        // [Signal]
        // delegate void StateIdle();

        public void Init(Vector2 cellCoords, FakeGrid grid, int colorId = 0, bool rndColors = false)
        {
            _cellCoords = cellCoords;
            _originalCoords = _cellCoords;
            _colorId = colorId;
            this.Scale = grid.CellSize / Texture.GetSize();
            _originalScale = this.Scale;

            Position = Globals.Utilities.CellCoordsToPosition(cellCoords, grid);
            _originalPosition = Position;
            Modulate = Globals.ColorManager.CurrentColorPalette.DefaultColor;
            if (rndColors)
            {
                _color = Globals.ColorManager.GetColorRandom(_colorId);
                return;
            }
            _color = Globals.ColorManager.GetColor(_colorId);
        }
        public void InitColor(int colorId = 0)
        {
            _colorId = colorId;
            _color = Globals.ColorManager.GetColor(_colorId);
        }
        public void Restart()
        {
            _flipped = false;
            Modulate = Globals.ColorManager.CurrentColorPalette.DefaultColor;
            _colorId = -1;
            _isOff = false;
            this.Scale = _originalScale;
            _state = Globals.BLOCKSTATE.IDLE;
        }
        public void Reset()
        {
            _flipped = false;
            Modulate = Globals.ColorManager.CurrentColorPalette.DefaultColor;
            _isOff = false;
            Scale = _originalScale;
            Position = _originalPosition;
            CellCoords = _originalCoords;
            _state = Globals.BLOCKSTATE.IDLE;
        }


        public void Copy(Block block)
        {
            _cellCoords = block.CellCoords;
            _colorId = block.ColorId;
            Scale = block.Scale;

            Position = block.Position;
            Modulate = block.Modulate;
            _color = block.Color;
        }
        public bool Swap(Block toBlock, bool tobeScaled = false)
        {
            _state = Globals.BLOCKSTATE.ANIMATING;
            toBlock.State = Globals.BLOCKSTATE.ANIMATING;

            Vector2 fromCoords = CellCoords;

            CellCoords = toBlock.CellCoords;
            toBlock.CellCoords = fromCoords;

            if (!tobeScaled)
            {
                return false;
            }

            if (IsOff)
            {
                toBlock.ZIndex = 1;
                return true;
            }

            if (toBlock.IsOff)
            {
                ZIndex = 1;
                return true;
            }

            ZIndex = 1;
            toBlock.ZIndex = -1;
            return true;


        }
        public bool Flip()
        {
            _flipped = !_flipped;
            _state = Globals.BLOCKSTATE.ANIMATING;

            return _flipped;

        }
        public void SwitchOff()
        {
            _isOff = true;
            _flipped = false;
            ZIndex = -1;

            _state = Globals.BLOCKSTATE.ANIMATING;
        }
        public void SetOff()
        {
            _isOff = true;
            _flipped = false;
            ZIndex = -1;

            Scale /= TweenManager.ScaleFactor;
            Modulate = Globals.ColorManager.CurrentColorPalette.OffColor;
        }

        public void SetState(Globals.BLOCKSTATE state)
        {
            _state = state;
            //EmitSignal(nameof(StateIdle));
        }

    }
}