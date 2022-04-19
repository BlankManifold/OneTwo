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

    }
    public enum GRIDSTATE
    {
        IDLE, CHECKING_COLOR, SWAPPING, COLLAPSING
    }

    public static class ColorPalette
    {
        public static Color DefaultColor = new Color(0.2f, 0.20f, 0.2f);
        static List<Color> _palette1 = new List<Color>()
        {
            new Color(0f, 0.058824f, 0.333333f),
            new Color(0f, 0f, 0f),
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
}
