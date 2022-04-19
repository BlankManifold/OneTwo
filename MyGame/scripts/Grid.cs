using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Main
{
    public class Grid : Node2D
    {
        [Export]
        private Vector2 _gridSize = new Vector2(4, 4);
        private int _numberOfColors = 4;
        private Vector2 _gridExtent;

        [Export]
        private int _cellBorder = 10;

        [Export]
        private int _cellSize = 64;

        private PackedScene _blockScene;
        private Godot.Collections.Array<Block> _blocks;
        private Node2D _blocksContainer;
        private Block[,] _blocksMatrix;
        private Vector2 _selectedCoords;
        private List<int> _colorsAuxList;
        private Label _label;

        private Vector2 _invalidCoords = new Vector2(-1, -1);


        public override void _Process(float _)
        {
            if (_selectedCoords == _invalidCoords)
            {
                _label.Text = "Selected: null";
                return;
            }
            _label.Text = $"Selected: {_selectedCoords}";
        }

        public override void _Ready()
        {
            _label = GetNode<Label>("Label");
            _blocksContainer = GetNode<Node2D>("Blocks");
            _blockScene = (PackedScene)ResourceLoader.Load("res://scene/Block.tscn");
            
            _gridExtent = _gridSize * _cellSize;
            _numberOfColors = (int)_gridSize.y;
            PopulateAuxColorArray();
            RandomizeAuxColorArray();

            _blocksMatrix = new Block[(int)_gridSize.x, (int)_gridSize.y];

            GenerateBlocks(_blocksContainer);

            // _blocks = new Godot.Collections.Array<Block>(_blocksContainer.GetChildren());
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("selectBlock"))
            {
                _selectedCoords = SelectCoords(GetLocalMousePosition());
                GD.Print($"ClickSelected: {_selectedCoords}");
            }
            if (@event.IsActionReleased("selectBlock"))
            {
                if (_selectedCoords != _invalidCoords)
                    DoMove(SelectCoords(GetLocalMousePosition()));

                _selectedCoords = _invalidCoords;
            }

            @event.Dispose();
        }

        private Block GetBlock(Vector2 cellCoords)
        {
            return _blocksMatrix[(int)cellCoords.x, (int)cellCoords.y];
        }
        private Block GetBlock(int i, int j)
        {
            return _blocksMatrix[i,j];
        }

        private void DoMove(Vector2 blockReleasedOn)
        {
            if (blockReleasedOn == _invalidCoords)
            {
                GD.Print($"ReleasedSelected: null");
                return;
            }

            GD.Print($"ReleasedSelected: {blockReleasedOn}");
            if (blockReleasedOn == _selectedCoords)
            {
                GetBlock(_selectedCoords).Flip();
                return;
            }

            SwapBlocks(blockReleasedOn);

        }
        private void SwapBlocks(Vector2 coordsReleasedOn)
        {
            if (!CheckValidSwap(coordsReleasedOn))
            {
                return;
            }

            Vector2 startPos = _selectedCoords * (_cellSize + _cellBorder);
            Vector2 endPos = coordsReleasedOn * (_cellSize + _cellBorder);

            Block selectedBlock = GetBlock(_selectedCoords);
            Block blockReleasedOn = GetBlock(coordsReleasedOn);

            _blocksMatrix[(int)coordsReleasedOn.x, (int)coordsReleasedOn.y] = selectedBlock;
            _blocksMatrix[(int)_selectedCoords.x, (int)_selectedCoords.y] = blockReleasedOn;

            selectedBlock.GoTo(endPos);
            blockReleasedOn.GoTo(startPos);
        }
        private Vector2 SelectCoords(Vector2 position)
        {
            if (!IsInGrid(position))
            {
                GD.Print($"Out: {position}");
                return new Vector2(-1, -1);
            }

            Vector2 cellCoords = PostionToCellCoords(position);
            GD.Print($"In: {position}, {cellCoords}");

            if (cellCoords.x == 0)
            {
                return _invalidCoords;
            }

            return cellCoords;//_blocksContainer.GetNode<Block>($"Block_{(int)cellCoords.x}_{(int)cellCoords.y}");
        }

        private bool CheckValidSwap(Vector2 coordsReleasedOn)
        {
            Vector2 coordsDiff = coordsReleasedOn - _selectedCoords;
            return (Math.Abs(coordsDiff.x) + Math.Abs(coordsDiff.y) <= 2 && Math.Abs(coordsDiff.x) < 2 && Math.Abs(coordsDiff.y) < 2);
        }
        private bool IsInGrid(Vector2 position)
        {
            return (position.x >= 0 && position.x <= _gridExtent.x && position.y >= 0 && position.y <= _gridExtent.y);
        }

        private Vector2 PostionToCellCoords(Vector2 position)
        {
            int row = (int)position.x / _cellSize;
            int col = (int)position.y / _cellSize;

            return new Vector2(row, col);
        }

        private void GenerateBlocks(Node2D blocksContainer)
        {
            Globals.ColorPalette.RandomizeColorList();

            for (int i = 0; i < _gridSize.x; i++)
            {
                for (int j = 0; j < _gridSize.y; j++)
                {
                    Block block = _blockScene.Instance<Block>();

                    int colorId = PickColorId(i, j);

                    block.Init(new Vector2(i, j), _cellSize, _cellBorder, colorId);
                    blocksContainer.AddChild(block);
                    _blocksMatrix[i, j] = block;

                    if (i == 0)
                    {
                        block.Flip();
                    }
                }
            }
        }

        private int PickColorId(int i, int j)
        {
            if (i == 0)
            {
                return j;
            }

            int colorId = _colorsAuxList[(i-1)*_numberOfColors + j];
            
            return colorId;            
        }

        private void PopulateAuxColorArray()
        {
             _colorsAuxList = new List<int>{};
            for (int i = 0; i < _numberOfColors; i++)
            {
                for (int j = 0; j < _numberOfColors-1; j++)
                {
                    _colorsAuxList.Add(i);
                }
            }

            _colorsAuxList = _colorsAuxList.OrderBy(item => Globals.RandomManager.rnd.Next()).ToList();
        }
        private void RandomizeAuxColorArray()
        {
            _colorsAuxList = _colorsAuxList.OrderBy(item => Globals.RandomManager.rnd.Next()).ToList();
        }
        
        public void Restart()
        {
            Globals.ColorPalette.RandomizeColorList();

            for (int i = 0; i < _gridSize.x; i++)
            {
                for (int j = 0; j < _gridSize.y; j++)
                {
                    Block block = GetBlock(i, j);
                    block.Restart();

                    int colorId = PickColorId(i, j);

                    block.Init(new Vector2(i, j), _cellSize, _cellBorder, colorId);

                    if (i == 0)
                    {
                        block.Flip();
                    }
                }
            }
        }

    }
}