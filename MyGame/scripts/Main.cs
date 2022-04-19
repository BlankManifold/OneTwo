using System;
using Godot;

namespace Main
{
    public class Main : Node2D
    {
        private Grid _grid;

        public override void _Ready()
        {
            _grid = GetNode<Grid>("Grid");
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
    }
}