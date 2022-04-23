using System;
using Godot;

namespace Main
{
    public class Block : Sprite
    {
        private bool _flipped = false;
        public bool Flipped { get { return _flipped;} set { _flipped = value; } }
        private bool _changedLayer = false;
        public bool ChangedLayer { get { return _changedLayer;}}
        private bool _isOff = false;
        public bool IsOff {get {return _isOff;}}
        
        private Globals.BLOCKSTATE _state = Globals.BLOCKSTATE.IDLE;
        public Globals.BLOCKSTATE State {get {return _state;} set {_state = value;}}



        private Tween _tween;
        public Tween Tween{ get {return _tween;}}
        private AnimationPlayer _animation;


        private Vector2 _cellCoords;
        public Vector2 CellCoords
        {
            get { return _cellCoords; }
            set { _cellCoords = value; }
        }

        private Color _color = Globals.ColorPalette.DefaultColor;
        public Color Color { get { return _color;}} 

        private int _colorId = -1;
        public int ColorId { get { return _colorId; }}

        [Signal]
        delegate void StateIdle();

        public void Init(Vector2 cellCoords, Vector2 cellSize, int colorId = 0)
        {
            _cellCoords = cellCoords;
            _colorId = colorId;
            this.Scale =  cellSize / Texture.GetSize();
  
            Position = Globals.Utilities.CellCoordsToPosition(cellCoords);
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
            _isOff = false;
        }


        public override void _Ready()
        {
            _tween = GetNode<Tween>("BlockTween");
            _animation = GetNode<AnimationPlayer>("AnimationPlayer");
        }

        // public override void _PhysicsProcess(float delta)
        // {
        //     Label label = GetNode<Label>("Label");
        //     label.Text = $"{_cellCoords}";
        // }


        public bool Swap(Block toBlock, bool tobeScaled = false)
        {
             _state = Globals.BLOCKSTATE.ANIMATING;
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
        
        public void SetState(Globals.BLOCKSTATE state)
        {
            _state = state;
            EmitSignal(nameof(StateIdle));
        }
    
    }
}