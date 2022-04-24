using System;
using Godot;

namespace Main
{
    public class Main : Node2D
    {
        private Grid _grid;
        // private GameUI _gameUI;
        private ControlTemplate _settingsControl;
        private Control _gridControl;
        private ControlTemplate _mainControl;
        private Tween _tween;

        public override void _Ready()
        {
            // _gameUI = GetNode<GameUI>("GridLayer/MainNode/GameUI");
            _gridControl = GetNode<Control>("GridLayer/MainControl/GridControl");
            _settingsControl = GetNode<ControlTemplate>("GridLayer/SettingsControl");
            _mainControl = GetNode<ControlTemplate>("GridLayer/MainControl");
            _tween = GetNode<Tween>("MainTween");


            PackedScene _gridScene = (PackedScene)ResourceLoader.Load("res://scene/Grid.tscn");
            _grid = _gridScene.Instance<Grid>();

            int sizeConstraint = (int)GetViewport().GetVisibleRect().Size.x - 200;
            Vector2 cellRatio = new Vector2(1f, 1f);

            _grid.Init(true, new Vector2(4, 6), new Vector2(64, 64) * cellRatio, new Vector2(10, 10), sizeConstraint);
            UpdateGridInfo();

            _gridControl.AddChild(_grid);

            //_mainControl.GetParent().MoveChild(_mainControl, 1);

            //CenterGridPosition();
            RotateGrid();

        }


        public async void _on_GameUI_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "RestartButton":
                    _grid.Restart();
                    break;

                case "SettingsButton":

                    TweenManager.ChangePanelSwap(_tween, _mainControl, _settingsControl);

                    _mainControl.Visible = true;
                    _settingsControl.Visible = true;

                    GD.Print($"{_mainControl.Name}:{_mainControl.GetIndex()}");
                    GD.Print($"{_settingsControl.Name}:{_settingsControl.GetIndex()}");

                    TweenManager.Start(_tween);

                    GD.Print($"{_mainControl.Name}:{_mainControl.GetIndex()}");
                    GD.Print($"{_settingsControl.Name}:{_settingsControl.GetIndex()}");


                    await ToSignal(_tween, "tween_all_completed");

                    GD.Print($"{_mainControl.Name}:{_mainControl.GetIndex()}");
                    GD.Print($"{_settingsControl.Name}:{_settingsControl.GetIndex()}");


                    _mainControl.UpdateState();
                    _settingsControl.UpdateState();

                    break;

                case "HelpButton":
                    break;
            }
        }
        public async void _on_SettingsControl_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "SoundOnButton":
                    break;

                case "MusicButton":

                case "BackButton":


                    TweenManager.ChangePanelSwap(_tween, _settingsControl, _mainControl);
                    
                    _mainControl.Visible = true;
                    _settingsControl.Visible = true;

                    TweenManager.Start(_tween);
                    await ToSignal(_tween, "tween_all_completed");

                    _mainControl.UpdateState();
                    _settingsControl.UpdateState();

                    break;
            }
        }

        // public override void _Input(InputEvent @event)
        // {
        //     if (@event.IsPressed())
        //     {
        //         //GD.Print($"LocalPos: {_grid.GetLocalMousePosition()}");
        //     }
        // }

        // public void CenterGridPosition()
        // {
        //     Vector2 offset = _grid.GridExtent / 2;
        //     _grid.GlobalPosition = _gameUI.BottomPosition - offset;
        // }

        public void RotateGrid()
        {
            _grid.Rotation = Mathf.Pi;
        }

        private void UpdateGridInfo()
        {
            Globals.GridInfo.UpdateGridInfo(_grid.GridSize, _grid.CellSize, _grid.CellBorder, _grid.Offset);
        }


    }
}