using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Globals
{
    public static class PackedScenes
    {
        public static PackedScene GridScene = (PackedScene)ResourceLoader.Load("res://scene/Grid.tscn");
        public static PackedScene FakeGridScene = (PackedScene)ResourceLoader.Load("res://scene/FakeGrid.tscn");
        public static PackedScene BlockScene = (PackedScene)ResourceLoader.Load("res://scene/Block.tscn");

    }
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
        static List<Color> _palette1rnd = new List<Color>()
        {
            new Color(0f, 0.058824f, 0.333333f),
            new Color(1f, 1f, 1f),
            new Color(0.67451f, 0.196078f, 0.207843f),
            new Color(0.015686f, 0.490196f, 0.329412f),
        };

        public static List<int> ColorList4x6 = new List<int> {  0,1,2,3,
                                                                0,1,2,3,
                                                                0,2,1,3,
                                                                0,2,1,3,
                                                                0,1,2,3,
                                                                0,1,2,3
                                                            };
        public static List<int> ColorList4x4 = new List<int> { 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3 };

        public static Color GetColor(int index)
        {
            return _palette1[index];
        }
        public static Color GetColorRandom(int index)
        {
            return _palette1rnd[index];
        }

        public static void RandomizeColorList()
        {
            _palette1rnd = _palette1.OrderBy(item => RandomManager.rnd.Next()).ToList();
        }
    }

    public struct RandomManager
    {
        public static RandomNumberGenerator rng = new RandomNumberGenerator();
        public static Random rnd = new Random();
    }

    public static class Utilities
    {
        public static Vector2 PositionToCellCoords(Vector2 position, Main.FakeGrid grid = null)
        {
            Vector2 cellSize = GridInfo.CellSize;
            Vector2 cellBorder = GridInfo.CellBorder;
            Vector2 offset = GridInfo.GridOffset;

            if (grid != null)
            {
                cellSize = grid.CellSize;
                cellBorder = grid.CellBorder;
                offset = grid.Offset;
            }

            Vector2 offsetPos = position + offset + cellSize / 2;

            int col = (int)(offsetPos.x / (cellSize.x + cellBorder.x));
            int row = (int)(offsetPos.y / (cellSize.y + cellBorder.y));

            // GD.Print($"Pos: {position} -> {new Vector2(col, row)}");


            return new Vector2(col, row);
        }
        public static Vector2 CellCoordsToPosition(Vector2 cellCoords, Main.FakeGrid grid = null)
        {
            Vector2 cellSize = GridInfo.CellSize;
            Vector2 cellBorder = GridInfo.CellBorder;
            Vector2 offset = GridInfo.GridOffset;

            if (grid != null)
            {
                cellSize = grid.CellSize;
                cellBorder = grid.CellBorder;
                offset = grid.Offset;
            }
            return cellCoords * (cellSize + cellBorder) - offset;
        }
        public static Vector2 CellCoordsToPosition(int col, int row, Main.FakeGrid grid = null)
        {
            Vector2 cellSize = GridInfo.CellSize;
            Vector2 cellBorder = GridInfo.CellBorder;
            Vector2 offset = GridInfo.GridOffset;

            if (grid != null)
            {
                cellSize = grid.CellSize;
                cellBorder = grid.CellBorder;
                offset = grid.Offset;
            }

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

        public static float ScaleFactor = 1.3f;

        public static void UpdateGridInfo(Vector2 gridSize, Vector2 cellSize, Vector2 cellBorder, Vector2 gridOffset)
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
