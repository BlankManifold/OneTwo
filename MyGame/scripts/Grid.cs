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
        public Vector2 GridSize { get { return _gridSize; } }

        [Export]
        private bool _offMovable = false;
        private int _numberOfColors = 4;
        private Vector2 _gridExtent;
        public Vector2 GridExtent { get { return _gridExtent; } }

        private Vector2 _offset;
        public Vector2 Offset { get { return _offset; } }

        [Export]
        private Vector2 _cellBorder = new Vector2(10, 10);
        public Vector2 CellBorder { get { return _cellBorder; } }

        private Vector2 _cellSize = new Vector2(64, 64);
        public Vector2 CellSize { get { return _cellSize; } }


        private bool _someoneFlipped = false;

        private PackedScene _blockScene;
        private Node2D _blocksContainer;
        private Block[,] _blocksMatrix;
        private  Godot.Collections.Array<Block> _blocks;
        private Vector2 _justPressedCoords;
        private Vector2 _selectedCoords;
        private List<int> _colorsAuxList;
        private List<int> _indexAuxList;
        private List<int> _remaingColorsList = new List<int>() { };
        private Globals.GRIDSTATE _gridState;
        private int _moves;

        private Tween _tween;

        private Vector2 _invalidCoords = new Vector2(-1, -1);

        [Signal]
        delegate void UpdateMoves(int moves);




        public void Init(bool offMovable, Vector2 gridSize, Vector2 cellSize, Vector2 cellBorder, int xConstraint)
        {
            _offMovable = offMovable;
            _gridSize = gridSize;
            _cellBorder = cellBorder;
            _cellSize = cellSize;
            _numberOfColors = (int)_gridSize[0];

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




        // public override void _Process(float _)
        // {

        // }
        public override void _Ready()
        {
            _tween = GetNode<Tween>("GridTween");
            _blocksContainer = GetNode<Node2D>("Blocks");
            _blockScene = (PackedScene)ResourceLoader.Load("res://scene/Block.tscn");

            PopulateAuxColorArray();
            RandomizeAuxColorArray();
            PopulateRemainingColorsArray();

            _blocksMatrix = new Block[(int)_gridSize[1], (int)_gridSize[0]];

            Connect(nameof(UpdateMoves), (GameUI)GetTree().GetNodesInGroup("GameUI")[0], "_on_Grid_UpdateMoves");

            GenerateBlocks();
        }
        public override void _UnhandledInput(InputEvent @event)
        {
            if (_gridState == Globals.GRIDSTATE.GENERATING)
            {
                @event.Dispose();
                return;
            }

            if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == 1)
            {

                if (mouseButtonEvent.IsPressed())
                {
                    Vector2 justPressedCoords = SelectCoords(GetLocalMousePosition());

                    if (IsActiveCoords(justPressedCoords))
                        _justPressedCoords = justPressedCoords;

                    mouseButtonEvent.Dispose();
                    return;
                }


                if (!mouseButtonEvent.IsPressed())
                {
                    Vector2 justReleasedOnCoords = SelectCoords(GetLocalMousePosition());
                    Vector2 justPressedCoords = _justPressedCoords;
                    Vector2 selectedCoords = _selectedCoords;
                    bool someoneFlipped = _someoneFlipped;

                    // IF RELEASED_ON_COORDS ARE VALID
                    if (IsActiveCoords(justReleasedOnCoords) && IsActiveCoords(justPressedCoords))
                        DoMove(justReleasedOnCoords, justPressedCoords, selectedCoords, someoneFlipped);

                    _justPressedCoords = _invalidCoords;

                    mouseButtonEvent.Dispose();
                    return;
                }

                mouseButtonEvent.Dispose();
                return;
            }

            @event.Dispose();
        }




        private Block GetBlock(Vector2 cellCoords)
        {
            if (cellCoords != _invalidCoords)
            {
                return _blocksMatrix[(int)cellCoords[1], (int)cellCoords[0]];
            }

            return null;
        }
        private Block GetBlock(int col, int row)
        {
            if (col == _invalidCoords[0] || row == _invalidCoords[1])
            {
                return _blocksMatrix[row, col];
            }

            return null;
        }
        private bool IsActiveCoords(Vector2 cellCoords)
        {
            if (cellCoords == _invalidCoords)
            {
                return false;
            }

            Block block = GetBlock(cellCoords);

            if (_offMovable)
            {
                return !(block.State == Globals.BLOCKSTATE.ANIMATING);
            }

            return !block.IsOff && !(block.State == Globals.BLOCKSTATE.ANIMATING);

        }
        private bool IsActiveCoords(int col, int row)
        {
            if (col == _invalidCoords[0] || row == _invalidCoords[1])
            {
                return false;
            }

            Block block = GetBlock(col, row);

            if (_offMovable)
            {
                return !(block.State == Globals.BLOCKSTATE.ANIMATING);
            }

            return !block.IsOff && !(block.State == Globals.BLOCKSTATE.ANIMATING);
        }
        private Vector2 SelectCoords(Vector2 position)
        {
            if (!IsInGrid(position))
            {
                GD.Print("Not valid");
                return _invalidCoords;
            }
            //GD.Print($"Position: {position}, Coords: {Globals.Utilities.PositionToCellCoords(position)}");
            Vector2 cellCoords = Globals.Utilities.PositionToCellCoords(position);

            if (cellCoords[1] == 0)
            {
                return _invalidCoords;
            }

            return cellCoords;
        }
        private void SetBlockMatrix(Vector2 cellCoords, Block block)
        {
            if (cellCoords != _invalidCoords)
            {
                _blocksMatrix[(int)cellCoords[1], (int)cellCoords[0]] = block;
            }
        }


        private async void DoMove(Vector2 justReleasedOnCoords, Vector2 justPressedCoords, Vector2 selectedCoords, bool someoneFlipped)
        {
            // IF RELEASED ON DIFFERENT BLOCK THAN PRESSED-BLOCK -> try SWAP
            if (justReleasedOnCoords != justPressedCoords)
            {
                if (CheckValidSwap(justReleasedOnCoords, justPressedCoords))
                {
                    float finalTime = SetUpSwapBlocks(justPressedCoords, justReleasedOnCoords);
                    _moves += 1;

                    // IF SWAPPED AND SOME BLOCK WAS FLIPPED -> FLIPPED BACK THAT BLOCK
                    if (someoneFlipped)
                    {
                        Vector2 toBeFlippedCoords = justReleasedOnCoords;

                        if (justReleasedOnCoords == selectedCoords)
                            toBeFlippedCoords = justPressedCoords;

                            SetUpUnselect(toBeFlippedCoords, finalTime);
                        _moves += 1;
                    }

                    TweenManager.Start(_tween, finalTime, GetBlock(justPressedCoords), GetBlock(justReleasedOnCoords));
                    EmitSignal(nameof(UpdateMoves), _moves);
                }
                return;
            }

            // IF RELEASED ON SAME BLOCK OF PRESSED-BLOCK AND BLOCK IS ON -> try FLIP
            if (justReleasedOnCoords == justPressedCoords && !GetBlock(justReleasedOnCoords).IsOff)
            {
                // IF NO BLOCK IS ALREADY FLIPPED -> FLIP THAT BLOCK
                if (!someoneFlipped)
                {
                    float finalTime = SetUpSelect(justPressedCoords, someoneFlipped);

                    int colorID = GetBlock(justPressedCoords).ColorId;
                    //...AND LAST COLOR BLOCK AND IS IN COL 1 -> CHECK IF SAME AS TARGET COLOR 
                    if (_remaingColorsList[colorID] == 2 && justPressedCoords[1] == 1)
                    {
                        Vector2 targetCoords = new Vector2(justPressedCoords[0], justPressedCoords[1] - 1);

                        //IF SAME AS TARGET COLOR -> SWITCH AND EXIT ANIMATION AND CHECK IF WIN 
                        if (CheckSameColor(justPressedCoords, targetCoords))
                        {
                            _moves += 1;
                            EmitSignal(nameof(UpdateMoves), _moves);

                            finalTime += SetUpSwitchOff(justPressedCoords, targetCoords, finalTime);
                            TweenManager.Start(_tween, finalTime, GetBlock(justPressedCoords), GetBlock(targetCoords));

                            if (CheckIfWin())
                            {
                                await ToSignal(_tween, "tween_all_completed");
                                Win();
                                return;
                            }

                            return;
                        }
                    }

                    TweenManager.Start(_tween, finalTime, GetBlock(justPressedCoords));
                    return;
                }

                // IF SOME BLOCK ALREADY FLIPPED...AND RELEASED ON ALREALDY FLIPPED BLOCK -> DO NOTHING
                if (someoneFlipped && selectedCoords == justPressedCoords)
                    return;

                // IF SOME BLOCK ALREADY FLIPPED...AND RELEASED ON NOT FLIPPED BLOCK AND IS ADJOINT -> FLIP THAT BLOCK AND CHECK COLORS...
                if (someoneFlipped && CheckIfAdjoint(justPressedCoords, selectedCoords))
                {
                    float finalTime = SetUpSelect(justPressedCoords);

                    //...AND NOT SOME COLOR -> WAIT AND FLIP BACK BOTH
                    if (!CheckSameColor(justPressedCoords, selectedCoords))
                    {
                        finalTime += SetUpUnselect(justPressedCoords, selectedCoords, finalTime);
                    }
                    else
                    {
                        finalTime += SetUpSwitchOff(justPressedCoords, selectedCoords, finalTime);
                    }

                    _moves += 1;
                    EmitSignal(nameof(UpdateMoves), _moves);
                    TweenManager.Start(_tween, finalTime, GetBlock(justPressedCoords), GetBlock(selectedCoords));
                    return;
                }

                return;
            }
        }


        private void Select(Vector2 coords, bool someoneFlipped = false, float delay = 0f)
        {
            float finalTime = SetUpSelect(coords, someoneFlipped, delay);
            TweenManager.Start(_tween, finalTime, GetBlock(coords));
        }
        private float SetUpSelect(Vector2 coords, bool someoneFlipped = false, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.SELECTING;
            if (!someoneFlipped)
            {
                _someoneFlipped = true;
                _selectedCoords = coords;
            }

            Block block = GetBlock(coords);
            block.Flip();

            return TweenManager.SelectModulate(_tween, block, delay);
        }
        private void Unselect(Vector2 coords, float delay = 0f)
        {
            float finalTime = SetUpUnselect(coords, delay);
            TweenManager.Start(_tween, finalTime, GetBlock(coords));

        }
        private float SetUpUnselect(Vector2 coords, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.UNSELECTING;
            _selectedCoords = _invalidCoords;
            _someoneFlipped = false;

            Block block = GetBlock(coords);
            block.Flip();

            return TweenManager.UnSelectModulate(_tween, block, delay);
        }
        private void Unselect(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {
            float finalTime = SetUpUnselect(coords1, coords2, delay);
            TweenManager.Start(_tween, finalTime, GetBlock(coords1), GetBlock(coords2));
        }
        private float SetUpUnselect(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.UNSELECTING;
            _selectedCoords = _invalidCoords;
            _someoneFlipped = false;

            Block block1 = GetBlock(coords1);
            Block block2 = GetBlock(coords2);
            block1.Flip();
            block2.Flip();

            return TweenManager.UnSelectModulate(_tween, block1, block2, delay);
        }
        private void SwitchOff(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {
            float finalTime = SetUpSwitchOff(coords1, coords2, delay);
            TweenManager.Start(_tween, finalTime, GetBlock(coords1), GetBlock(coords2));
        }
        private float SetUpSwitchOff(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.INACTIVATING;
            _someoneFlipped = false;
            _selectedCoords = _invalidCoords;

            Block block1 = GetBlock(coords1);
            Block block2 = GetBlock(coords2);

            _remaingColorsList[block1.ColorId] -= 2;

            block1.SwitchOff();
            block2.SwitchOff();

            return TweenManager.SwitchOffScaleModulate(_tween, block1, block2, delay);
        }
        private void SwapBlocks(Vector2 fromCoords, Vector2 toCoords, float delay = 0f)
        {
            float finalTime = SetUpSwapBlocks(fromCoords, toCoords, delay);

            TweenManager.Start(_tween, finalTime, GetBlock(fromCoords), GetBlock(toCoords));
        }
        private float SetUpSwapBlocks(Vector2 fromCoords, Vector2 toCoords, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.SWAPPING;

            Block fromBlock = GetBlock(fromCoords);
            Block toBlock = GetBlock(toCoords);

            bool diagonalSwap = false;
            if (fromCoords[0] != toCoords[0] && fromCoords[1] != toCoords[1])
                diagonalSwap = true;
    
            bool tobeScaled = fromBlock.Swap(toBlock, CheckIfScaledSwap(fromBlock, toBlock, diagonalSwap));
            float finalTime = TweenManager.SwapHovering(_tween, fromBlock, toBlock, tobeScaled, diagonalSwap, delay);
            
            SetBlockMatrix(toCoords, fromBlock);
            SetBlockMatrix(fromCoords, toBlock);

            return finalTime;
        }
        private void Win()
        {
            Restart();
            return;
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

            for (int row = 0; row < _gridSize[1]; row++)
            {
                for (int col = 0; col < _gridSize[0]; col++)
                {
                    Block block = _blocksMatrix[row, col];
                    int colorId = PickColorId(col, row);

                    block.Restart();
                    block.Init(new Vector2(col, row), _cellSize, colorId);

                    if (row == 0)
                    {
                        block.Flipped = true;
                        block.Modulate = block.Color;
                    }
                }
            }


        }



        private bool CheckIfAdjointsOn(Vector2 coords)
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
        private bool CheckIfScaledSwap(Vector2 fromCoords, Vector2 toCoords, bool diagonalSwap)
        {
            return CheckIfScaledSwap(GetBlock(fromCoords), GetBlock(toCoords), diagonalSwap);
        }
        private bool CheckIfScaledSwap(Block fromBlock, Block toBlock, bool diagonalSwap)
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
        private bool CheckValidSwap(Vector2 coords1, Vector2 coords2)
        {
            // IF NOT ADJOINT
            if (!CheckIfAdjoint(coords1, coords2)) return false;

            // IF SOMEONE FLIPPED AND TRY TO SWAP OTHER 2 BLOCK (MUST SWAP FLIPPED BLOCK)
            if (_someoneFlipped && coords1 != _selectedCoords && coords2 != _selectedCoords) return false;

            // IF BOTH BLOCK ARE INACTIVE
            Block block1 = GetBlock(coords1);
            Block block2 = GetBlock(coords2);
            if (block1.IsOff && block2.IsOff) return false;

            return true;
        }
        private bool CheckSameColor(Vector2 coords1, Vector2 coords2)
        {
            return (GetBlock(coords1).ColorId == GetBlock(coords2).ColorId);
        }
        private bool CheckIfWin()
        {
            return (_remaingColorsList.Sum() == 0);
        }
        private bool CheckIfAdjoint(Vector2 coords1, Vector2 coords2)
        {
            Vector2 coordsDiff = coords1 - coords2;
            bool positionBool = (Math.Abs(coordsDiff[0]) + Math.Abs(coordsDiff[1]) <= 2 && Math.Abs(coordsDiff[0]) < 2 && Math.Abs(coordsDiff[1]) < 2);

            bool isIdle = (GetBlock(coords1).State == Globals.BLOCKSTATE.IDLE && GetBlock(coords2).State == Globals.BLOCKSTATE.IDLE);

            return positionBool && isIdle;
        }
        private bool IsInGrid(Vector2 position)
        {
            Vector2 testPosition = position + _offset + _cellSize / 2;

            int remainderX = (int)(testPosition.x % (_cellSize.x + _cellBorder.x));
            int remainderY = (int)(testPosition.y % (_cellSize.y + _cellBorder.y));
            bool inBound = (testPosition.x >= 0) && (testPosition.x < _gridExtent.x) && (testPosition.y >= 0) && (testPosition.y < _gridExtent.y);
            bool onBlokcs = (remainderX <= _cellSize.x) && (remainderY <= _cellSize.y);
            return (inBound && onBlokcs);
        }



        private async void GenerateBlocks()
        {
            Globals.ColorPalette.RandomizeColorList();

            for (int row = 0; row < _gridSize[1]; row++)
            {
                for (int col = 0; col < _gridSize[0]; col++)
                {
                    Block block = _blockScene.Instance<Block>();

                    int colorId = PickColorId(col, row);

                    block.Init(new Vector2(col, row), _cellSize, colorId);
                    _blocksContainer.AddChild(block);
                    _blocksMatrix[row, col] = block;
                    
                    block.Visible = false;
                    if (row == 0)
                    {
                        block.Flipped = true;
                        block.Modulate = block.Color;
                    }
                }
            }

            _blocks = new Godot.Collections.Array<Block>(GetTree().GetNodesInGroup("Block"));
            TweenManager.GenerateBlocks(_tween, _blocks);
            
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
            _gridState = Globals.GRIDSTATE.GENERATING;
            TweenManager.Start(_tween);
        }
        private int PickColorId(int col, int row)
        {
            if (row == 0)
            {
                return col;
            }

            int colorId = _colorsAuxList[(row - 1) * _numberOfColors + col];

            return colorId;
        }
        private void PopulateAuxColorArray()
        {
            _colorsAuxList = new List<int> { };
            for (int col = 0; col < _numberOfColors; col++)
            {
                for (int row = 0; row < (int)_gridSize[1] - 1; row++)
                {
                    _colorsAuxList.Add(col);
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
            for (int col = 0; col < _numberOfColors; col++)
            {
                _remaingColorsList.Add((int)_gridSize[1]);
            }
        }


        public void _on_GridTween_tween_all_completed()
        {
            _gridState = Globals.GRIDSTATE.IDLE;
        }
    }
}