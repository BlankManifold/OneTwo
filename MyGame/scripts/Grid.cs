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

        private bool _someoneFlipped = false;

        private PackedScene _blockScene;
        private Godot.Collections.Array<Block> _blocks;
        private Node2D _blocksContainer;
        private Block[,] _blocksMatrix;
        private Vector2 _justPressedCoords;
        private Vector2 _selectedCoords;
        private List<int> _colorsAuxList;
        private List<int> _remaingColorsList = new List<int>() { };
        private Globals.GRIDSTATE _gridState;
        private Label _label;
        private int _moves;

        private Tween _tween;

        private Vector2 _invalidCoords = new Vector2(-1, -1);

        [Signal]
        delegate void UpdateMoves(int moves);


        public override void _Process(float _)
        {
            if (_justPressedCoords == _invalidCoords)
            {
                //_label.Text = "Selected: null";
                return;
            }
            //_label.Text = $"Selected: {_justPressedCoords}";
        }
        public override void _Ready()
        {
            _label = GetNode<Label>("Label");
            _tween = GetNode<Tween>("GridTween");
            _blocksContainer = GetNode<Node2D>("Blocks");
            _blockScene = (PackedScene)ResourceLoader.Load("res://scene/Block.tscn");

            _gridExtent = _gridSize * _cellSize;
            _numberOfColors = (int)_gridSize.y;
            PopulateAuxColorArray();
            RandomizeAuxColorArray();
            PopulateRemainingColorsArray();

            _blocksMatrix = new Block[(int)_gridSize.x, (int)_gridSize.y];

            GenerateBlocks();

            Connect(nameof(UpdateMoves), (GameUI)GetTree().GetNodesInGroup("GameUI")[0], "_on_Grid_UpdateMoves");

            // _blocks = new Godot.Collections.Array<Block>(_blocksContainer.GetChildren());
        }
        public override void _UnhandledInput(InputEvent @event)
        {
            if (_gridState != Globals.GRIDSTATE.IDLE)
            {
                @event.Dispose();
                return;
            }

            if (@event.IsActionPressed("selectBlock"))
            {
                Vector2 justPressedCoords = SelectCoords(GetLocalMousePosition());

                if (GetBlock(justPressedCoords) == null)
                {
                    @event.Dispose();
                    return;
                }

                _justPressedCoords = justPressedCoords;

                GD.Print($"ClickSelected: {_justPressedCoords}");
                @event.Dispose();
                return;
            }

            if (@event.IsActionReleased("selectBlock"))
            {
                //IF VALID: IT IS IN GRID
                if (_justPressedCoords != _invalidCoords)
                {
                    Vector2 justReleasedOnCoords = SelectCoords(GetLocalMousePosition());

                    // IF RELEASED_ON_COORDS ARE VALID
                    if (GetBlock(justReleasedOnCoords) != null)
                    {
                        DoMove(justReleasedOnCoords);
                    }
                }

                _justPressedCoords = _invalidCoords;

                @event.Dispose();
                return;
            }

            @event.Dispose();
        }



        private Block GetBlock(Vector2 cellCoords)
        {
            if (cellCoords != _invalidCoords)
            {
                return _blocksMatrix[(int)cellCoords.x, (int)cellCoords.y];
            }

            return null;
        }
        private void SetBlockMatrix(Vector2 cellCoords, Block block = null)
        {
            if (cellCoords != _invalidCoords)
            {
                _blocksMatrix[(int)cellCoords.x, (int)cellCoords.y] = block;
            }
        }
        private Block GetBlock(int i, int j)
        {
            if (i != _invalidCoords.x && j != _invalidCoords.y)
            {
                return _blocksMatrix[i, j];
            }

            return null;
        }

        private async void DoMove(Vector2 justReleasedOnCoords)
        {
            // IF RELEASED ON DIFFERENT BLOCK THAN PRESSED-BLOCK -> try SWAP
            if (justReleasedOnCoords != _justPressedCoords)
            {
                // IF ALREADY ONE FLIPPED AND THE FLIPPED BLOCK IS NOT INTERACTING IN THIS SWAP -> DO NOTHING 
                if (_someoneFlipped && _justPressedCoords != _selectedCoords && justReleasedOnCoords != _selectedCoords)
                {
                    return;
                }

                if (SwapBlocks(justReleasedOnCoords) && _someoneFlipped)
                {
                    Vector2 justPressedCoords = _justPressedCoords;

                    await ToSignal(_tween, "tween_all_completed");
                    Vector2 toBeFlippedCoords = justReleasedOnCoords;

                    if (justReleasedOnCoords == _selectedCoords)
                    {
                        toBeFlippedCoords = justPressedCoords;
                    }

                    GetBlock(toBeFlippedCoords).Flip();
                    _someoneFlipped = false;
                    _selectedCoords = _invalidCoords;
                    _moves += 1;
                }
                _moves += 1;
                EmitSignal(nameof(UpdateMoves), _moves);
                return;
            }

            // IF RELEASED ON SAME BLOCK OF PRESSED-BLOCK -> FLIP THAT BLOCK
            if (justReleasedOnCoords == _justPressedCoords)
            {
                // IF NO BLOCK IS ALREADY FLIPPED -> FLIP THAT BLOCK
                if (!_someoneFlipped)
                {
                    Block flippedBlock = GetBlock(_justPressedCoords);
                    flippedBlock.Flip();
                    _selectedCoords = _justPressedCoords;
                    _someoneFlipped = true;

                    //...AND LAST COLOR BLOCK AND IS IN COL 1 -> CHECK IF SAME AS TARGET COLOR 
                    if (_remaingColorsList[flippedBlock.ColorId] == 2 && _selectedCoords.x == 1)
                    {
                        Vector2 targetCoords = new Vector2(_justPressedCoords.x - 1, _justPressedCoords.y);
                        Vector2 justPressedCoords = _justPressedCoords;

                        //IF SAME AS TARGET COLOR -> COLLAPSING AND CHECK IF WIN 
                        if (CheckSameColor(_justPressedCoords, targetCoords))
                        {
                            _moves += 1;
                            EmitSignal(nameof(UpdateMoves), _moves);

                            _someoneFlipped = false;
                            _selectedCoords = _invalidCoords;
                            _gridState = Globals.GRIDSTATE.COLLAPSING;
                            _remaingColorsList[GetBlock(_justPressedCoords).ColorId] -= 2;


                            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

                            if (CheckIfWin())
                            {
                                Win();
                                return;
                            }

                            GetBlock(justPressedCoords).QueueFree();
                            GetBlock(targetCoords).QueueFree();
                            SetBlockMatrix(justPressedCoords, null);
                            SetBlockMatrix(targetCoords, null);


                            Collapse(1);
                            return;
                        }
                    }

                    return;
                }

                // IF SOME BLOCK ALREADY FLIPPED...AND RELEASED ON ALREALDY FLIPPED BLOCK -> DO NOTHING
                if (_selectedCoords == _justPressedCoords)
                {
                    return;
                }

                // IF SOME BLOCK ALREADY FLIPPED...AND RELEASED ON NOT FLIPPED BLOCK AND IS ADJOINT -> FLIP THAT BLOCK AND CHECK COLORS...
                if (CheckIfAdjoint(_justPressedCoords, _selectedCoords))
                {
                    _moves += 1;
                    EmitSignal(nameof(UpdateMoves), _moves);

                    _someoneFlipped = false;
                    GetBlock(_justPressedCoords).Flip();

                    //...AND NOT SOME COLOR -> WAIT AND FLIP BACK BOTH
                    Vector2 justPressedCoords = _justPressedCoords;
                    if (!CheckSameColor(_justPressedCoords))
                    {
                        _gridState = Globals.GRIDSTATE.CHECKING_COLOR;

                        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");

                        GetBlock(justPressedCoords).Flip();
                        GetBlock(_selectedCoords).Flip();
                        _selectedCoords = _invalidCoords;

                        _gridState = Globals.GRIDSTATE.IDLE;
                        return;
                    }

                    //...AND SOME COLOR -> COLLAPSE BLOCKS
                    _gridState = Globals.GRIDSTATE.COLLAPSING;
                    _remaingColorsList[GetBlock(_selectedCoords).ColorId] -= 2;
                    await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
                    GetBlock(justPressedCoords).QueueFree();
                    GetBlock(_selectedCoords).QueueFree();
                    SetBlockMatrix(justPressedCoords, null);
                    SetBlockMatrix(_selectedCoords, null);

                    Collapse();
                }


                return;
            }
        }

        private async void Collapse(int limitX = 1)
        {
            if (CollapseHorizontally(limitX))
            {
                _tween.Start();
                await ToSignal(_tween, "tween_all_completed");

                if (CollapseVertically())
                {
                    _gridState = Globals.GRIDSTATE.COLLAPSING;
                    _tween.Start();
                }

                return;
            }

            if (CollapseVertically())
            {
                _tween.Start();
                return;
            }

            _gridState = Globals.GRIDSTATE.IDLE;
        }
        private bool CollapseHorizontally(int limitX = 1)
        {
            bool collapsable = false;

            for (int j = 0; j < (int)_gridSize.y; j++)
            {
                int gap = 0;

                for (int i = limitX; i < _gridSize.x; i++)
                {
                    Block currentBlock = GetBlock(i, j);
                    if (currentBlock == null)
                    {
                        gap++;
                        continue;
                    }

                    if (gap != 0 && currentBlock != null)
                    {
                        SetGoTo(currentBlock, CellCoordsToPosition(i - gap, j), 0.4f);

                        _blocksMatrix[i - gap, j] = currentBlock;
                        _blocksMatrix[i, j] = null;
                        currentBlock.CellCoords = new Vector2(i - gap, j);
                        collapsable = true;

                        continue;
                    }

                }
            }

            return collapsable;
        }

        private bool CollapseVertically()
        {
            bool collapsable = false;

            for (int i = 0; i < (int)_gridSize.x; i++)
            {
                int gap = 0;

                for (int j = (int)_gridSize.y - 1; j >= 0; j--)
                {
                    Block currentBlock = GetBlock(i, j);
                    if (currentBlock == null)
                    {
                        gap++;
                        continue;
                    }

                    if (gap != 0 && currentBlock != null)
                    {
                        SetGoTo(currentBlock, CellCoordsToPosition(i, j + gap), 0.4f);

                        _blocksMatrix[i, j + gap] = currentBlock;
                        _blocksMatrix[i, j] = null;
                        currentBlock.CellCoords = new Vector2(i, j + gap);
                        collapsable = true;

                        continue;
                    }
                }
            }

            return collapsable;
        }
        private bool SwapBlocks(Vector2 coordsReleasedOn)
        {
            if (!CheckIfAdjoint(coordsReleasedOn, _justPressedCoords))
            {
                return false;
            }

            Vector2 startPos = _justPressedCoords * (_cellSize + _cellBorder);
            Vector2 endPos = coordsReleasedOn * (_cellSize + _cellBorder);

            Block selectedBlock = GetBlock(_justPressedCoords);
            Block blockReleasedOn = GetBlock(coordsReleasedOn);

            _blocksMatrix[(int)coordsReleasedOn.x, (int)coordsReleasedOn.y] = selectedBlock;
            _blocksMatrix[(int)_justPressedCoords.x, (int)_justPressedCoords.y] = blockReleasedOn;
            selectedBlock.CellCoords = coordsReleasedOn;
            blockReleasedOn.CellCoords = _justPressedCoords;


            _tween.InterpolateProperty(selectedBlock, "position", selectedBlock.Position, endPos, 0.2f);
            _tween.InterpolateProperty(blockReleasedOn, "position", blockReleasedOn.Position, startPos, 0.2f);
            _tween.Start();
            _gridState = Globals.GRIDSTATE.SWAPPING;

            // selectedBlock.GoTo(endPos);
            // blockReleasedOn.GoTo(startPos);

            return true;
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

        private bool CheckSameColor(Vector2 coordsReleasedOn)
        {
            return (GetBlock(coordsReleasedOn).ColorId == GetBlock(_selectedCoords).ColorId);
        }
        private bool CheckSameColor(Vector2 coords1, Vector2 coords2)
        {
            return (GetBlock(coords1).ColorId == GetBlock(coords2).ColorId);
        }

        private bool CheckIfWin()
        {
            return (_remaingColorsList.Sum() == 0);
        }
        private void Win()
        {
            Restart();
            return;
        }

        private Vector2 CheckSameColorTarget(Vector2 justPressedCoords)
        {
            Vector2 targetCoords = _invalidCoords;

            if (GetBlock(justPressedCoords).ColorId == GetBlock(targetCoords).ColorId)
            {
                targetCoords = new Vector2(justPressedCoords.x - 1, justPressedCoords.y);
            };

            return targetCoords;
        }
        private bool CheckIfAdjoint(Vector2 coords1, Vector2 coords2)
        {
            Vector2 coordsDiff = coords1 - coords2;
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
        private Vector2 CellCoordsToPosition(Vector2 cellCoords)
        {
            return cellCoords * (_cellSize + _cellBorder);
        }
        private Vector2 CellCoordsToPosition(int i, int j)
        {
            return new Vector2(i, j) * (_cellSize + _cellBorder);
        }

        private void GenerateBlocks()
        {
            Globals.ColorPalette.RandomizeColorList();

            for (int i = 0; i < _gridSize.x; i++)
            {
                for (int j = 0; j < _gridSize.y; j++)
                {
                    Block block = _blockScene.Instance<Block>();

                    int colorId = PickColorId(i, j);

                    block.Init(new Vector2(i, j), _cellSize, _cellBorder, colorId);
                    _blocksContainer.AddChild(block);
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

            int colorId = _colorsAuxList[(i - 1) * _numberOfColors + j];

            return colorId;
        }

        private void PopulateAuxColorArray()
        {
            _colorsAuxList = new List<int> { };
            for (int i = 0; i < _numberOfColors; i++)
            {
                for (int j = 0; j < (int)_gridSize.x - 1; j++)
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

        private void PopulateRemainingColorsArray()
        {
            _remaingColorsList.Clear();
            for (int i = 0; i < _numberOfColors; i++)
            {
                _remaingColorsList.Add((int)_gridSize.x);
            }
        }
        public void Restart()
        {
            _moves = 0;
            EmitSignal(nameof(UpdateMoves), 0);
            
            _someoneFlipped = false;
            _justPressedCoords = _invalidCoords;
            _selectedCoords = _invalidCoords;
            _gridState = Globals.GRIDSTATE.IDLE;

            Globals.ColorPalette.RandomizeColorList();
            RandomizeAuxColorArray();
            PopulateRemainingColorsArray();

            for (int i = 0; i < _gridSize.x; i++)
            {
                for (int j = 0; j < _gridSize.y; j++)
                {
                    Block block = GetBlock(i, j);

                    if (block == null)
                    {
                        block = _blockScene.Instance<Block>();
                        _blocksContainer.AddChild(block);
                    }

                    _blocksMatrix[i, j] = block;
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

        public void GoTo(Block block, Vector2 position)
        {
            _tween.InterpolateProperty(block, "position", block.Position, position, 0.2f);
            _tween.Start();
        }
        public void SetGoTo(Block block, Vector2 position, float duration = 0.2f)
        {
            _tween.InterpolateProperty(block, "position", block.Position, position, duration);
        }
        public void _on_GridTween_tween_all_completed()
        {
            _gridState = Globals.GRIDSTATE.IDLE;
        }
    }
}