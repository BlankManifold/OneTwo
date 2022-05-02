using System;
using System.Collections.Generic;
using Godot;
using BoolMatrix = Godot.Collections.Array<Godot.Collections.Array<bool>>;

namespace Main
{
    public class FakeGrid : Node2D
    {   
        protected AudioManager _audioManager;
        public AudioManager AudioManager { get {return _audioManager;}}
        
        
        [Export]
        public int _soundSelectDB;
        [Export]
        public int _soundSwapDB;
        [Export]
        public int _soundSwitchOffDB;
        [Export]
        public int _soundWinDB;

        [Export]
        private AudioStream _soundSelect;
        
        [Export]
        private AudioStream _soundWin;
        
        [Export]
        private AudioStream _soundSwap;
                
        [Export]
        private AudioStream _soundSwitchOff;

        [Export]
        protected Vector2 _gridSize = new Vector2(4, 4);
        public Vector2 GridSize { get { return _gridSize; } }

        [Export]
        protected bool _offMovable = false;
        protected int _numberOfColors = 4;
        protected Vector2 _gridExtent;
        public Vector2 GridExtent { get { return _gridExtent; } }

        protected Vector2 _invalidCoords = new Vector2(-1, -1);
        protected Vector2 _offset;
        public Vector2 Offset { get { return _offset; } }

        [Export]
        protected Vector2 _cellBorder = new Vector2(10, 10);
        public Vector2 CellBorder { get { return _cellBorder; } }

        protected Vector2 _cellSize = new Vector2(64, 64);
        public Vector2 CellSize { get { return _cellSize; } }


        protected bool _animateGeneration = true;
        protected List<int> _colorsAuxList;

        protected PackedScene _blockScene;
        public PackedScene BlockScene { get { return _blockScene; }}
        protected Node2D _blocksContainer;
        protected Block[,] _blocksMatrix;
        protected Godot.Collections.Array<Block> _blocks;
        protected Tween _tween;
        public Tween Tween { get { return _tween; } }

        protected List<Block> _auxBlocks = new List<Block>() { };
        

        public void Init(bool offMovable, Vector2 gridSize, Vector2 cellSize, Vector2 cellBorder, int xConstraint, bool animateGeneration = true, List<int> colorsAuxList = null)
        {
            _offMovable = offMovable;
            _gridSize = gridSize;
            _cellBorder = cellBorder;
            _cellSize = cellSize;
            _numberOfColors = (int)_gridSize[0];
            _animateGeneration = animateGeneration;
            _colorsAuxList = colorsAuxList;

            SetGridExtent(xConstraint);
            Vector2 centerCoords = _gridSize / 2f - 0.5f * Vector2.One;

            _offset = (_cellSize + _cellBorder) * centerCoords;

        }
        public void SetGridExtent(int xConstraint)
        {
            float xMaxSize = xConstraint / _gridSize[0] - _cellBorder.x;
            float factor = xMaxSize / _cellSize.x;
            _cellSize *= factor;

            _gridExtent = _gridSize * (_cellSize + _cellBorder) - _cellBorder;
        }



        public override void _Ready()
        {
            _audioManager = (AudioManager)GetTree().GetNodesInGroup("AudioManager")[0];
            _tween = GetNode<Tween>("GridTween");
            _blocksContainer = GetNode<Node2D>("Blocks");
            _blockScene = (PackedScene)ResourceLoader.Load("res://scene/Block.tscn");

            _blocksMatrix = new Block[(int)_gridSize[1], (int)_gridSize[0]];

            CreateBlocks(true);
        }


        public AudioStream GetAudioStream(string name)
        {
            switch (name)
            {
                case "Select":
                    return _soundSelect;
                case "SwitchOff":
                    return _soundSwitchOff;
                case "Swap":
                    return _soundSwap;
                case "Win":
                    return _soundWin;
                    
                default:
                    return null;
                    
            }
        }
        public Block GetBlock(Vector2 cellCoords)
        {
            if (cellCoords != _invalidCoords)
            {
                return _blocksMatrix[(int)cellCoords[1], (int)cellCoords[0]];
            }

            return null;
        }
        public Block GetBlock(int col, int row)
        {
            if (col != _invalidCoords[0] && row != _invalidCoords[1])
            {
                return _blocksMatrix[row, col];
            }

            return null;
        }
        protected void SetBlockMatrix(Vector2 cellCoords, Block block)
        {
            if (cellCoords != _invalidCoords)
            {
                _blocksMatrix[(int)cellCoords[1], (int)cellCoords[0]] = block;
            }
        }

        public List<Block> GetRow(int row)
        {
            List<Block> blocks = new List<Block>(){};
            for (int i = 0; i < _gridSize.x; i++)
            {
                blocks.Add(_blocksMatrix[row, i]);
            }
            return blocks;
        }

        protected void Select(Vector2 coords, bool someoneFlipped = false, float delay = 0f)
        {
            float finalTime = SetUpSelect(coords, someoneFlipped, delay);
            TweenManager.Start(_tween, finalTime, GetBlock(coords));

        }
        protected virtual float SetUpSelect(Vector2 coords, bool someoneFlipped = false, float delay = 0f)
        {
            Block block = GetBlock(coords);
            block.Flip();

            return TweenManager.SelectModulate(_tween, this, block, delay);
        }
        protected void Unselect(Vector2 coords, float delay = 0f)
        {
            float finalTime = SetUpUnselect(coords, delay);
            TweenManager.Start(_tween, finalTime, GetBlock(coords));

        }
        protected virtual float SetUpUnselect(Vector2 coords, float delay = 0f)
        {


            Block block = GetBlock(coords);
            block.Flip();

            return TweenManager.UnSelectModulate(_tween, block, delay);
        }
        protected void Unselect(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {
            float finalTime = SetUpUnselect(coords1, coords2, delay);
            TweenManager.Start(_tween, finalTime, GetBlock(coords1), GetBlock(coords2));
        }
        protected virtual float SetUpUnselect(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {


            Block block1 = GetBlock(coords1);
            Block block2 = GetBlock(coords2);
            block1.Flip();
            block2.Flip();

            return TweenManager.UnSelectModulate(_tween, block1, block2, delay);
        }
        protected void SwitchOff(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {
            float finalTime = SetUpSwitchOff(coords1, coords2, delay);
            TweenManager.Start(_tween, finalTime, GetBlock(coords1), GetBlock(coords2));
        }
        protected virtual float SetUpSwitchOff(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {
            Block block1 = GetBlock(coords1);
            Block block2 = GetBlock(coords2);

            block1.SwitchOff();
            block2.SwitchOff();

            return TweenManager.SwitchOffScaleModulate(_tween, this, block1, block2, delay);
        }
        protected void SwapBlocks(Vector2 fromCoords, Vector2 toCoords, float delay = 0f)
        {
            float finalTime = SetUpSwapBlocks(fromCoords, toCoords, delay);

            TweenManager.Start(_tween, finalTime, GetBlock(fromCoords), GetBlock(toCoords));
        }
        protected virtual float SetUpSwapBlocks(Vector2 fromCoords, Vector2 toCoords, float delay = 0f)
        {

            Block fromBlock = GetBlock(fromCoords);
            Block toBlock = GetBlock(toCoords);

            bool diagonalSwap = false;
            if (fromCoords[0] != toCoords[0] && fromCoords[1] != toCoords[1])
                diagonalSwap = true;

            bool tobeScaled = fromBlock.Swap(toBlock, CheckIfScaledSwap(fromBlock, toBlock, diagonalSwap));
            float finalTime = TweenManager.SwapHovering(_tween, this, fromBlock, toBlock, tobeScaled, diagonalSwap, delay);

            SetBlockMatrix(toCoords, fromBlock);
            SetBlockMatrix(fromCoords, toBlock);

            return finalTime;
        }

        protected bool CheckIfAdjointsOn(Vector2 coords)
        {
            Vector2 diffCoords = Vector2.Zero;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }

                    diffCoords[0] = i;
                    diffCoords[1] = j;

                    if (!GetBlock(coords + diffCoords).IsOff)
                    {
                        return true;
                    };
                }
            }
            return false;
        }
        protected bool CheckSameColor(Vector2 coords1, Vector2 coords2)
        {
            return (GetBlock(coords1).ColorId == GetBlock(coords2).ColorId);
        }
        protected bool CheckIfScaledSwap(Vector2 fromCoords, Vector2 toCoords, bool diagonalSwap)
        {
            return CheckIfScaledSwap(GetBlock(fromCoords), GetBlock(toCoords), diagonalSwap);
        }
        protected bool CheckIfScaledSwap(Block fromBlock, Block toBlock, bool diagonalSwap)
        {
            if (!fromBlock.IsOff && !toBlock.IsOff)
            {
                return true;
            }

            if (diagonalSwap)
            {
                Vector2 coordsDiff = toBlock.CellCoords - fromBlock.CellCoords;
                Vector2 coord1 = fromBlock.CellCoords;
                Vector2 coord2 = fromBlock.CellCoords;

                coord1[0] += coordsDiff[0];
                coord2[1] += coordsDiff[1];
                return (!GetBlock(coord1).IsOff || !GetBlock(coord2).IsOff);
            }

            return false;
        }

        protected virtual bool CheckIfAdjoint(Vector2 coords1, Vector2 coords2)
        {
            if (coords1 == _invalidCoords || coords2 == _invalidCoords)
            {
                return false;
            }

            Vector2 coordsDiff = coords1 - coords2;
            bool positionBool = (Math.Abs(coordsDiff[0]) + Math.Abs(coordsDiff[1]) <= 2 && Math.Abs(coordsDiff[0]) < 2 && Math.Abs(coordsDiff[1]) < 2);

            return positionBool;
        }

        public void CreateBlocks(bool visible = false, bool rndColor = false)
        {
            for (int row = 0; row < _gridSize[1]; row++)
            {
                for (int col = 0; col < _gridSize[0]; col++)
                {
                    Block block = _blockScene.Instance<Block>();

                    int colorId = PickColorId(col, row, _colorsAuxList);

                    block.Init(new Vector2(col, row), this, colorId, rndColor);
                    _blocksContainer.AddChild(block);
                    _blocksMatrix[row, col] = block;

                    block.Visible = visible;
                    if (row == 0)
                    {
                        block.Flipped = true;
                        block.Modulate = block.Color;
                    }
                }
            }
        }
        public void Reset(bool visible = false, bool rndColor = false)
        {
            for (int row = 0; row < _gridSize[1]; row++)
            {
                for (int col = 0; col < _gridSize[0]; col++)
                {
                    Block block = GetBlock(col, row);

                    block.Reset();

                    block.Visible = visible;
                    if (row == 0)
                    {
                        block.Flipped = true;
                        block.Modulate = block.Color;
                    }
                }
            }
        }
        public void ResetOff(BoolMatrix offArray, bool visible = false, bool rndColor = false)
        {
            for (int row = 0; row < _gridSize[1]; row++)
            {
                for (int col = 0; col < _gridSize[0]; col++)
                {
                    Block block = GetBlock(col, row);
                    block.Reset();

                    if (!offArray[row][col])
                        block.SetOff();

                    block.Visible = visible;
                    if (row == 0)
                    {
                        block.Flipped = true;
                        block.Modulate = block.Color;
                        continue;
                    }


                }
            }
        }
        protected int PickColorId(int col, int row, List<int> colorList)
        {
            if (row == 0)
            {
                return col;
            }

            int colorId = colorList[(row - 1) * _numberOfColors + col];

            return colorId;
        }

        public void AddAuxBlock(Block block)
        {
            _auxBlocks.Add(block);
        }
        public void DeleteAllAuxBlocks()
        {
            foreach (Block auxBlock in _auxBlocks)
            {
                if (!auxBlock.IsQueuedForDeletion())
                    auxBlock.CallDeferred("queue_free");
            }
            _auxBlocks.Clear();
        }
    }
}