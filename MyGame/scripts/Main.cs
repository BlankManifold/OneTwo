using System;
using Godot;

namespace Main
{
    public class Main : Node2D
    {
        private Grid _grid;
        // private GameUI _gameUI;
        private ControlTemplate _settingsControl;
        private ControlTemplate _mainControl;
        private ControlTemplate _helpControl;
        private Control _gridControl;
        private Tween _tween;

        public override void _Ready()
        {
            // _gameUI = GetNode<GameUI>("GridLayer/MainNode/GameUI");
            _gridControl = GetNode<Control>("GridLayer/MainControl/GridControl");
            _settingsControl = GetNode<ControlTemplate>("GridLayer/SettingsControl");
            _mainControl = GetNode<ControlTemplate>("GridLayer/MainControl");
            _helpControl = GetNode<ControlTemplate>("GridLayer/HelpControl");
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


        public void _on_GameUI_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "RestartButton":
                    _grid.Restart();
                    break;

                case "SettingsButton":

                    ChangePanel(_settingsControl, _mainControl);
                    break;

                case "HelpButton":
                    ChangePanel(_helpControl, _mainControl);
                    break;
            }
        }
        public void _on_SettingsControl_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "SoundOnButton":
                    break;

                case "MusicButton":
                    break;

                case "BackButton":
                    ChangePanel(_mainControl, _settingsControl);
                    break;
            }
        }
        public void _on_HelpControl_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "BackButton":
                    ChangePanel(_mainControl, _helpControl);
                    break;
            }
        }

    
        public void RotateGrid()
        {
            _grid.Rotation = Mathf.Pi;
        }

        private void UpdateGridInfo()
        {
            Globals.GridInfo.UpdateGridInfo(_grid.GridSize, _grid.CellSize, _grid.CellBorder, _grid.Offset);
        }

        private async void ChangePanel(ControlTemplate controlIn, ControlTemplate controlOut)
        {
            TweenManager.ChangePanelSwap(_tween, controlOut, controlIn);

            controlOut.Visible = true;
            controlIn.Visible = true;

            TweenManager.Start(_tween);

            await ToSignal(_tween, "tween_all_completed");

            controlOut.UpdateState();
            controlIn.UpdateState();
        }


    }
}