using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Globals
{
    public enum BLOCKTYPE
    {

    }
    public enum GAMESTATE
    {

    }
    public enum BLOCKSTATE
    {
        IDLE, ANIMATING
    }
    public enum GRIDSTATE
    {
        IDLE, SWAPPING, INACTIVATING, SELECTING, UNSELECTING, GENERATING
    }

    public static class ColorPalette
    {
        public static Color DefaultColor = new Color(0.2f, 0.20f, 0.2f);
        public static Color NoColor = new Color(0.2f, 0.20f, 0.2f, 0f);
        public static Color OffColor = new Color(0f, 0f, 0f);
        static List<Color> _palette1 = new List<Color>()
        {
            new Color(0f, 0.058824f, 0.333333f),
            new Color(1f, 1f, 1f),
            new Color(0.67451f, 0.196078f, 0.207843f),
            new Color(0.015686f, 0.490196f, 0.329412f),
        };

        public static Color GetRandomColor()
        {
            int index = RandomManager.rng.RandiRange(0, _palette1.Count - 1);
            return _palette1[index];
        }
        public static Color GetColor(int index)
        {
            return _palette1[index];
        }

        public static List<Color> RandomizedColorList()
        {
            return _palette1.OrderBy(item => RandomManager.rnd.Next()).ToList();
        }
        public static void RandomizeColorList()
        {
            _palette1 = _palette1.OrderBy(item => RandomManager.rnd.Next()).ToList();
        }
    }

    public struct RandomManager
    {
        public static RandomNumberGenerator rng = new RandomNumberGenerator();
        public static Random rnd = new Random();
    }

    public static class Utilities
    {
         public static Vector2 PositionToCellCoords(Vector2 position)
        {
            Vector2 cellSize = GridInfo.CellSize;
            Vector2 cellBorder = GridInfo.CellBorder;
            Vector2 offset = GridInfo.GridOffset;

            Vector2 offsetPos = position + offset + cellSize/2;

            int col = (int)(offsetPos.x / (cellSize.x + cellBorder.x));
            int row = (int)(offsetPos.y / (cellSize.y + cellBorder.y));

           // GD.Print($"Pos: {position} -> {new Vector2(col, row)}");


            return new Vector2(col, row);
        }
        public static Vector2 CellCoordsToPosition(Vector2 cellCoords)
        {
            Vector2 cellSize = GridInfo.CellSize;
            Vector2 cellBorder = GridInfo.CellBorder;
            Vector2 offset = GridInfo.GridOffset;
            // GD.Print($"Cells: {cellCoords} -> {cellCoords * (cellSize + cellBorder) - offset}");
            return cellCoords * (cellSize + cellBorder) - offset;
        }
        public static Vector2 CellCoordsToPosition(int col, int row)
        {
            Vector2 cellSize = GridInfo.CellSize;
            Vector2 cellBorder = GridInfo.CellBorder;
            Vector2 offset = GridInfo.GridOffset;
            // GD.Print($"Cells: {new Vector2(col, row)} -> { new Vector2(col, row) * (cellSize + cellBorder) - offset}");

            return new Vector2(col, row) * (cellSize + cellBorder) - offset;
        }
    }
    public static class GridInfo
    {
        public static Vector2 CellSize;
        public static Vector2 CellBorder;
        public static Vector2 GridSize;
        public static Vector2 GridOffset;
        public static float DiagFactor;

        public static void UpdateGridInfo( Vector2 gridSize, Vector2 cellSize, Vector2 cellBorder, Vector2 gridOffset)
        {
            CellSize = cellSize;
            CellBorder = cellBorder;
            GridSize = gridSize;
            GridOffset = gridOffset;
            //TODO: factor for x and y direction -> must know direction of swapping
            DiagFactor = Mathf.Sqrt2;
        }
    }
}
