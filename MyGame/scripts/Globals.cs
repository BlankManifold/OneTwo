using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Globals
{
    // public static class PackedScenes
    // {
    //     public static PackedScene GridScene = (PackedScene)ResourceLoader.Load("res://scene/Grid.tscn");
    //     public static PackedScene FakeGridScene = (PackedScene)ResourceLoader.Load("res://scene/FakeGrid.tscn");
    //     public static PackedScene BlockScene = (PackedScene)ResourceLoader.Load("res://scene/Block.tscn");

    // }
    public enum BLOCKSTATE
    {
        IDLE, ANIMATING
    }
    public enum GRIDSTATE
    {
        IDLE, ANIMATING, GENERATING, WINNING, WIN
    }

    public struct ColorPalette
    {
        public List<Color> Palette;
        public Color DefaultColor;
        public Color OffColor;
        public Color NoColor;
        public Color BackgroundColorMain;
        public Color BackgroundColorSecondary;
        public Color ButtonColor;

        public ColorPalette(List<Color> palette, Color defaultColor, Color offColor, Color noColor, Color backgroundColorMain, Color backgroundColorSecondary, Color buttonColor)
        {
            Palette = palette;
            DefaultColor = defaultColor;
            OffColor = offColor;
            NoColor = noColor;
            BackgroundColorMain = backgroundColorMain;
            BackgroundColorSecondary = backgroundColorSecondary;
            ButtonColor = buttonColor;
        }
        public ColorPalette(string defaultColor, string offColor, string noColor, string backgroundColorMain, string backgroundColorSecondary, string buttonColor, params string[] paletteColors)
        {
            Palette = ColorManager.CPalette(paletteColors);
            DefaultColor =  ColorManager.CHex(defaultColor);
            OffColor = ColorManager.CHex(offColor);
            NoColor = ColorManager.CHex(noColor);
            BackgroundColorMain = ColorManager.CHex(backgroundColorMain);
            BackgroundColorSecondary = ColorManager.CHex(backgroundColorSecondary);
            ButtonColor = ColorManager.CHex(buttonColor);
        }
    }
    public static class ColorManager
    {
        
        public static Color CHex(string hex)
        {
            return new Color(hex);
        }
        public static List<Color> CPalette(params Color[] colors)
        {
            List<Color> palette = new List<Color>();

            foreach (Color c in colors)
            {
                palette.Add(c);
            }

            return palette;
        }
        public static List<Color> CPalette(params string[] colors)
        {
            List<Color> palette = new List<Color>();

            foreach (string c in colors)
            {
                palette.Add(CHex(c));
            }

            return palette;
        }

        static ColorPalette _paletteJorge = new ColorPalette
            (
               "a3ada2","c2baa6","00000000","e4e0cf","c2baa6","f1efe4", "6ea08e","656f76","cbbb45","e78c54"
            );


        public static ColorPalette CurrentColorPalette = _paletteJorge;
        public static ColorPalette CurrentColorPaletteRnd = CurrentColorPalette;

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
            return CurrentColorPalette.Palette[index];
        }

        public static Color GetColorRandom(int index)
        {
            return CurrentColorPaletteRnd.Palette[index];
        }

        public static void RandomizeColorList()
        {
            CurrentColorPaletteRnd.Palette = CurrentColorPalette.Palette.OrderBy(item => RandomManager.rnd.Next()).ToList();
        }
        public static List<Color> GetRandomizeColorList()
        {
           return CurrentColorPalette.Palette.OrderBy(item => RandomManager.rnd.Next()).ToList();
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

        public static void UpdateGridInfo(Vector2 gridSize, Vector2 cellSize, Vector2 cellBorder, Vector2 gridOffset)
        {
            CellSize = cellSize;
            CellBorder = cellBorder;
            GridSize = gridSize;
            GridOffset = gridOffset;
            // factor for x and y direction -> must know direction of swapping
            DiagFactor = Mathf.Sqrt2;
        }
    }
}
