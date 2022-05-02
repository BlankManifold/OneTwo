using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Main
{
    public class RealGrid : FakeGrid
    {

        private bool _someoneFlipped = false;
        private Vector2 _justPressedCoords = new Vector2(-1, -1);
        private Vector2 _selectedCoords = new Vector2(-1, -1);
        private List<int> _indexAuxList;
        private List<int> _remaingColorsList = new List<int>() { };
        private Globals.GRIDSTATE _gridState = Globals.GRIDSTATE.IDLE;
        public Globals.GRIDSTATE GridState { get { return _gridState; } }
        private int _moves;

        private Node _winLabel;
        public Node WinLabel { get { return _winLabel; } }

        [Signal]
        delegate void UpdateMoves(int moves);
        [Signal]
        delegate void WinState(bool winning);


        public override void _Ready()
        {

            _winLabel = GetParent().GetNode<Node>("WinLabel");
            _audioManager = (AudioManager)GetTree().GetNodesInGroup("AudioManager")[0];
            _tween = GetNode<Tween>("GridTween");
            _blocksContainer = GetNode<Node2D>("Blocks");
            _blockScene = (PackedScene)ResourceLoader.Load("res://scene/Block.tscn");



            PopulateAuxColorArray();
            RandomizeAuxColorArray();
            PopulateRemainingColorsArray();



            _blocksMatrix = new Block[(int)_gridSize[1], (int)_gridSize[0]];

            Connect(nameof(UpdateMoves), (GameUI)GetTree().GetNodesInGroup("GameUI")[0], "_on_Grid_UpdateMoves");
            Connect(nameof(WinState), (Main)GetTree().GetNodesInGroup("Main")[0], "_on_Grid_WinState");



            if (_animateGeneration)
            {
                GenerateBlocks();
            }




            SetwinLabel();



        }
        public override void _UnhandledInput(InputEvent @event)
        {
            if (_gridState == Globals.GRIDSTATE.GENERATING || _gridState == Globals.GRIDSTATE.WINNING)
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
                    Vector2 releasedPosition = GetLocalMousePosition();
                    Vector2 justReleasedOnCoords = SelectCoords(releasedPosition);
                    Vector2 justPressedCoords = _justPressedCoords;
                    Vector2 selectedCoords = _selectedCoords;

                    if (justReleasedOnCoords != justPressedCoords && !CheckIfAdjoint(justReleasedOnCoords, justPressedCoords))
                    {
                        justReleasedOnCoords = SelectDraggedToCoords(releasedPosition, justPressedCoords);
                    }


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



        private bool IsActiveCoords(Vector2 cellCoords)
        {
            if (cellCoords == _invalidCoords)
            {
                return false;
            }

            Block block = GetBlock(cellCoords);

            if (_offMovable)
            {
                return block.State == Globals.BLOCKSTATE.IDLE;
            }

            return !block.IsOff && block.State == Globals.BLOCKSTATE.IDLE;

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
                return block.State == Globals.BLOCKSTATE.IDLE;
            }

            return !block.IsOff && block.State == Globals.BLOCKSTATE.IDLE;
        }
        private Vector2 SelectCoords(Vector2 position)
        {
            if (!IsInGrid(position))
            {
                return _invalidCoords;
            }

            Vector2 cellCoords = Globals.Utilities.PositionToCellCoords(position, this);

            if (cellCoords[1] == 0)
            {
                return _invalidCoords;
            }

            return cellCoords;
        }
        private Vector2 SelectCoordsUnSafe(Vector2 position)
        {
            Vector2 cellCoords = Globals.Utilities.PositionToCellCoords(position, this);

            return cellCoords;
        }

        private Vector2 SelectDraggedToCoords(Vector2 releasedPosition, Vector2 justPressedCoords)
        {
            Vector2 justReleasedOnCoords = SelectCoordsUnSafe(releasedPosition);
            Vector2 coordsDiff = justReleasedOnCoords - justPressedCoords;

            if (Mathf.Abs(coordsDiff.x) > 3 || Mathf.Abs(coordsDiff.y) > 3)
            {
                return _invalidCoords;
            }

            coordsDiff.x = Mathf.Clamp(coordsDiff.x, -1, 1);
            coordsDiff.y = Mathf.Clamp(coordsDiff.y, -1, 1);

            justReleasedOnCoords = _justPressedCoords + coordsDiff;

            if (!IsCoordsValid(justReleasedOnCoords) || justReleasedOnCoords.y == 0)
            {
                return _invalidCoords;
            }

            return justReleasedOnCoords;
        }


        private void DoMove(Vector2 justReleasedOnCoords, Vector2 justPressedCoords, Vector2 selectedCoords, bool someoneFlipped)
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
                                //await ToSignal(_tween, "tween_all_completed");
                                Win(finalTime - 0.3f);
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
        private void Win(float delay = 0.0f)
        {
            int oldHighscore = SaveManager.LoadHighscore();
            if (oldHighscore == -1 || oldHighscore > _moves)
            {
                SaveManager.SaveHighscore(Math.Min(100, _moves));
            }
            SetUpWin(delay);
            TweenManager.Start(_tween);
            EmitSignal(nameof(WinState), true);
            return;
        }
        public async void Restart()
        {

            if (_gridState == Globals.GRIDSTATE.WIN)
            {
                foreach (Label label in _winLabel.GetChildren())
                {
                    label.Visible = false;
                }
            }

            _moves = 0;
            EmitSignal(nameof(UpdateMoves), 0);

            if (_tween.IsActive())
            {
                await ToSignal(_tween, "tween_all_completed");
            }

            _someoneFlipped = false;
            _justPressedCoords = _invalidCoords;
            _selectedCoords = _invalidCoords;
            _gridState = Globals.GRIDSTATE.IDLE;
            _auxBlocks.Clear();

            Globals.ColorManager.RandomizeColorList();
            RandomizeAuxColorArray();
            PopulateRemainingColorsArray();


            for (int row = 0; row < _gridSize[1]; row++)
            {
                for (int col = 0; col < _gridSize[0]; col++)
                {
                    Block block = _blocksMatrix[row, col];
                    int colorId = PickColorId(col, row, _colorsAuxList);

                    block.Restart();
                    block.Init(new Vector2(col, row), this, colorId, true);

                    if (row == 0)
                    {
                        block.Flipped = true;
                        block.Modulate = block.Color;
                    }
                }
            }
        }
        private async void GenerateBlocks()
        {
            Globals.ColorManager.RandomizeColorList();
            CreateBlocks(false, true);

            _blocks = new Godot.Collections.Array<Block>(GetTree().GetNodesInGroup("Block"));
            TweenManager.GenerateBlocks(_tween, _blocks);

            _gridState = Globals.GRIDSTATE.GENERATING;
            await ToSignal(GetTree().CreateTimer(1f), "timeout");

            TweenManager.Start(_tween);
        }



        protected override float SetUpSelect(Vector2 coords, bool someoneFlipped = false, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.ANIMATING;
            if (!someoneFlipped)
            {
                _someoneFlipped = true;
                _selectedCoords = coords;
            }

            return base.SetUpSelect(coords, someoneFlipped, delay);
        }
        protected override float SetUpUnselect(Vector2 coords, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.ANIMATING;
            _selectedCoords = _invalidCoords;
            _someoneFlipped = false;

            return base.SetUpUnselect(coords, delay);
        }
        protected override float SetUpUnselect(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.ANIMATING;
            _selectedCoords = _invalidCoords;
            _someoneFlipped = false;

            return base.SetUpUnselect(coords1, coords2, delay);
        }
        protected override float SetUpSwitchOff(Vector2 coords1, Vector2 coords2, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.ANIMATING;
            _someoneFlipped = false;
            _selectedCoords = _invalidCoords;
            _remaingColorsList[GetBlock(coords1).ColorId] -= 2;

            return base.SetUpSwitchOff(coords1, coords2, delay);
        }
        protected override float SetUpSwapBlocks(Vector2 fromCoords, Vector2 toCoords, float delay = 0f)
        {
            _gridState = Globals.GRIDSTATE.ANIMATING;

            Block fromBlock = GetBlock(fromCoords);
            Block toBlock = GetBlock(toCoords);

            return base.SetUpSwapBlocks(fromCoords, toCoords, delay);
        }
        private float SetUpWin(float delay = 0.0f)
        {
            _gridState = Globals.GRIDSTATE.WINNING;

            return TweenManager.Win(_tween, this, delay);
        }
        private void SetwinLabel()
        {
            int i = 0;
            foreach (Label label in _winLabel.GetChildren())
            {
                label.RectSize = _cellSize;
                label.RectGlobalPosition = GetBlock(i, 2).GlobalPosition - _cellSize / 2;
                i++;
            }

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
        private bool CheckIfWin()
        {
            return (_remaingColorsList.Sum() == 0);
        }
        protected override bool CheckIfAdjoint(Vector2 coords1, Vector2 coords2)
        {

            bool positionBool = base.CheckIfAdjoint(coords1, coords2);

            if (!positionBool)
            {
                return false;
            }

            bool isIdle = (GetBlock(coords1).State == Globals.BLOCKSTATE.IDLE && GetBlock(coords2).State == Globals.BLOCKSTATE.IDLE);

            return isIdle;
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
        private bool IsCoordsValid(Vector2 coords)
        {
            if (coords.x < 0 || coords.y < 0)
            {
                return false;
            }

            if (coords.x >= _gridSize.x || coords.y >= _gridSize.y)
            {
                return false;
            }

            return true;

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
            if (_gridState == Globals.GRIDSTATE.WINNING)
            {
                _gridState = Globals.GRIDSTATE.WIN;
                EmitSignal(nameof(WinState), false);
                return;
            }

            _gridState = Globals.GRIDSTATE.IDLE;
        }
    }
}