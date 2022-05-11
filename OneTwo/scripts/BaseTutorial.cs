using Godot;
using BoolMatrix = Godot.Collections.Array<Godot.Collections.Array<bool>>;
using BoolArray = Godot.Collections.Array<bool>;

namespace Main
{
    public class BaseTutorial : ControlTemplate
    {
        protected FakeGrid _grid;
        protected Tween _tween;
        protected Label _label;
        protected AnimationPlayer _animationPlayer;

        protected int _helpIndex = 0;

        protected BoolMatrix _resetOffArray = new BoolMatrix {new BoolArray {true, true,true,true},
                                                            new BoolArray {false,true,false,true},
                                                            new BoolArray {false,true,false,false},
                                                            new BoolArray {false,false,true,false},
                                                            new BoolArray {true,false,true,false},
                                                            new BoolArray {false,false,true,true}
                                                            };
        protected delegate void HelpTweenDelegate(float delay = 0.0f);
        protected HelpTweenDelegate _helpTweenFunction;

        // 1. select two adjacent blocks to find matching colors
        // 2. click and drag to swap two adjacent blocks. Swapping after selecting will cost an extra move 
        // 3. active blocks can also be swapped with deactivated ones
        // 4. after pairing all blocks of one color, you can match the remaining one with the colorful block
        // 5. the remaining block must be vertically aligned to be matched

        [Export(PropertyHint.MultilineText)]
        protected string _helpTip0 = "1. select two adjacent blocks to find matching colors";
        [Export(PropertyHint.MultilineText)]
        protected string _helpTip1 = "2. click and drag to swap two adjacent blocks. Swapping after selecting will cost an extra move";
        [Export(PropertyHint.MultilineText)]
        protected string _helpTip2 = "3. active blocks can also be swapped with deactivated ones";
        [Export(PropertyHint.MultilineText)]
        protected string _helpTip3 = "4. after pairing all blocks of one color, you can match the remaining one with the colorful block";

        [Export(PropertyHint.MultilineText)]
        protected string _helpTip4 = "5. the remaining block must be vertically aligned to be matched";


        public override void _Ready()
        {
            base._Ready();
            _label = GetNode<Label>("HelpLabel");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        }

        public void InstanceGrid(Vector2 gridSize, Vector2 cellSize, Vector2 cellBorder, Vector2 cellRatio, int sizeConstraint)
        {

            _grid = Globals.PackedScenes.FakeGridScene.Instance<FakeGrid>();
            // _grid = ((PackedScene)ResourceLoader.Load("res://scene/FakeGrid.tscn")).Instance<FakeGrid>();

            _grid.Init(true, gridSize, cellSize * cellRatio, cellBorder, sizeConstraint, false, Globals.ColorManager.ColorList4x6);
            _grid.Rotation = Mathf.Pi;

            GetNode<Control>("GridControl").AddChild(_grid);

            if (gridSize.x == 4 && gridSize.y == 6)
            {
                _helpTweenFunction = Help4x6;
            }

            _tween = _grid.Tween;
            _tween.Repeat = true;
        }

        private void Help4x6(float delay = 0.0f)
        {
            float totalTime = 0.0f;
            switch (_helpIndex)
            {
                case 0:
                    totalTime = TweenManager.Help4x6Tip0(_grid, _tween, delay);
                    _tween.InterpolateCallback(_grid, totalTime - 0.3f, "Reset", true, false);
                    _label.Text = _helpTip0;
                    break;
                case 1:
                    totalTime = TweenManager.Help4x6Tip1(_grid, _tween, delay);
                    _tween.InterpolateCallback(_grid, totalTime - 0.3f, "Reset", true, false);
                    _label.Text = _helpTip1;
                    break;
                case 2:
                    _grid.ResetOff(_resetOffArray, true, false);
                    totalTime = TweenManager.Help4x6Tip2(_grid, _tween, delay);
                    _tween.InterpolateCallback(_grid, totalTime - 0.3f, "ResetOff", _resetOffArray, true, false);
                    _label.Text = _helpTip2;
                    break;
                case 3:
                    _grid.ResetOff(_resetOffArray, true, false);
                    totalTime = TweenManager.Help4x6Tip3(_grid, _tween, delay);
                    _tween.InterpolateCallback(_grid, totalTime - 0.3f, "ResetOff", _resetOffArray, true, false);
                    _label.Text = _helpTip3;
                    break;
                case 4:
                    _label.Text = _helpTip4;
                    break;
            }
            _tween.Start();
        }

        public void StartHelpTween(float delay = 1.0f)
        {
            _helpTweenFunction(delay);
            _animationPlayer.Play("HelpButtonModulate");
        }
        public void StopHelpTween()
        {
            _tween.RemoveAll();
            _grid.DeleteAllAuxBlocks();
            _grid.Reset(true, false);
        }
        public void StopHelp()
        {
            StopHelpTween();
            _animationPlayer.Stop();
        }

        public void ChangeTip()
        {
            if (_helpIndex != 4)
            {
                StopHelpTween();
            }

            _helpTweenFunction(0.5f);
        }

        public void _on_NextButton_pressed()
        {
            _helpIndex++;
            if (_helpIndex > 4)
            {
                _helpIndex = 0;
            }

            ChangeTip();

        }
        public void _on_PreviousButton_pressed()
        {
            _helpIndex--;
            if (_helpIndex < 0)
            {
                _helpIndex = 4;
            }

            ChangeTip();

        }
    }
}