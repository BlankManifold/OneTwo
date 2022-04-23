using System;
using Godot;

namespace Main
{
    public class Main : Node2D
    {
        private Grid _grid;
        private GameUI _gameUI;

        public override void _Ready()
        {
            _gameUI = GetNode<GameUI>("GameUILayer/GameUI");

            PackedScene _gridScene = (PackedScene)ResourceLoader.Load("res://scene/Grid.tscn");
            _grid = _gridScene.Instance<Grid>();
            
            int sizeConstraint = (int)GetViewport().GetVisibleRect().Size.x - 200;
            Vector2 cellRatio = new Vector2(1f,1f);

            _grid.Init(true, new Vector2(4, 4), new Vector2(64,64)*cellRatio, new Vector2(10,10), sizeConstraint);
            UpdateGridInfo();
            
            GetNode<Control>("GridLayer/GridControlNode").AddChild(_grid);
            
        
            //RotateGrid();
            //CenterGridPosition();

        }

        public void _on_GameUI_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "RestartButton":
                    _grid.Restart();
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

        public void CenterGridPosition()
        {
            Vector2 offset = _grid.GridExtent/2;
            _grid.GlobalPosition = _gameUI.BottomPosition - offset;
        }

        public void RotateGrid()
        {
            _grid.Rotation = 3*Mathf.Pi/2;
        }

        private void UpdateGridInfo()
        {
            Globals.GridInfo.UpdateGridInfo(_grid.GridSize, _grid.CellSize, _grid.CellBorder, _grid.Offset);
        }


    }
}