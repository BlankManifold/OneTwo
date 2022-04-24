using Godot;

namespace Main
{
    public static class TweenManager
    {
        public static Vector2 Zeros = Vector2.Zero;
        public static Vector2 Ones = Vector2.One;
        public static float SelectModulate(Tween tween, Block block, float delay)
        {
            float modulateTime = 0.2f;
            tween.InterpolateProperty(block, "modulate", Globals.ColorPalette.DefaultColor, block.Color, modulateTime, Tween.TransitionType.Sine, delay: delay);

            return modulateTime + delay;
        }
        public static float UnSelectModulate(Tween tween, Block block, float delay)
        {
            float modulateTime = 0.2f;
            float totalDelay = delay;
            tween.InterpolateProperty(block, "modulate", block.Color, Globals.ColorPalette.DefaultColor, modulateTime, Tween.TransitionType.Sine, delay: totalDelay);

            return modulateTime + totalDelay;
        }
        public static float UnSelectModulate(Tween tween, Block block1, Block block2, float delay)
        {
            float modulateTime = 0.2f;
            float totalDelay = delay + 0.2f;
            tween.InterpolateProperty(block1, "modulate", block1.Color, Globals.ColorPalette.DefaultColor, modulateTime, Tween.TransitionType.Sine, delay: totalDelay);
            tween.InterpolateProperty(block2, "modulate", block2.Color, Globals.ColorPalette.DefaultColor, modulateTime, Tween.TransitionType.Sine, delay: totalDelay);

            return modulateTime + totalDelay;
        }
        public static float SwitchOffScaleModulate(Tween tween, Block block1, Block block2, float delay)
        {
            float scaleFactor = 1.4f;
            float scaleTime = 0.5f;
            float modulateTime = 0.5f;

            tween.InterpolateProperty(block1, "scale", block1.Scale, block1.Scale / scaleFactor, scaleTime, Tween.TransitionType.Sine, delay: delay);
            tween.InterpolateProperty(block2, "scale", block2.Scale, block2.Scale / scaleFactor, scaleTime, Tween.TransitionType.Sine, delay: delay);

            tween.InterpolateProperty(block1, "modulate", block1.Color, Globals.ColorPalette.OffColor, modulateTime, Tween.TransitionType.Linear, delay: delay);
            tween.InterpolateProperty(block2, "modulate", block1.Color, Globals.ColorPalette.OffColor, modulateTime, Tween.TransitionType.Linear, delay: delay);

            return delay + scaleTime;
        }
        public static float SwapHovering(Tween tween, Block fromBlock, Block toBlock, bool tobeScaled, bool diagonalSwap, float delay)
        {

            float scaleFactor = 1.3f;
            float scaleTime = 0.0f;
            float swapTime = 0.2f;

            Vector2 fromPos = Globals.Utilities.CellCoordsToPosition(toBlock.CellCoords);
            Vector2 toPos = Globals.Utilities.CellCoordsToPosition(fromBlock.CellCoords);

            if (diagonalSwap)
            {
                scaleTime *= Globals.GridInfo.DiagFactor;
                swapTime *= Globals.GridInfo.DiagFactor;
            }

            if (tobeScaled)
            {
                scaleTime = 0.1f;

                if (!fromBlock.IsOff)
                {
                    tween.InterpolateProperty(fromBlock, "scale", fromBlock.Scale, fromBlock.Scale * scaleFactor, scaleTime, Tween.TransitionType.Sine, delay: delay);
                    tween.InterpolateProperty(fromBlock, "scale", fromBlock.Scale * scaleFactor, fromBlock.Scale, scaleTime, Tween.TransitionType.Sine, delay: delay + scaleTime + swapTime);
                    tween.InterpolateCallback(fromBlock, 2 * scaleTime + swapTime, "set", "z_index", 0);
                }

                if (!toBlock.IsOff)
                {
                    if (fromBlock.IsOff)
                    {
                        scaleFactor = 1 / scaleFactor;
                    }
                    tween.InterpolateProperty(toBlock, "scale", toBlock.Scale, toBlock.Scale / scaleFactor, scaleTime, Tween.TransitionType.Sine);
                    tween.InterpolateProperty(toBlock, "scale", toBlock.Scale / scaleFactor, toBlock.Scale, scaleTime, Tween.TransitionType.Sine, delay: delay + scaleTime + swapTime);
                    tween.InterpolateCallback(toBlock, 2 * scaleTime + swapTime, "set", "z_index", 0);
                }
            }

            tween.InterpolateProperty(fromBlock, "position", fromPos, toPos, swapTime, Tween.TransitionType.Sine, delay: delay + scaleTime);
            tween.InterpolateProperty(toBlock, "position", toPos, fromPos, swapTime, Tween.TransitionType.Sine, delay: delay + scaleTime);

            return delay + 2 * scaleTime + swapTime;
        }

        public static float GenerateBlocks(Tween tween, Godot.Collections.Array<Block> blocks)
        {
            blocks.Shuffle();
            float scaleTime = 0.2f;
            float modulateTime = 0.2f;
            float delayEach = 0.1f;
            float delayNext = (2 * scaleTime - 2 * delayEach) * 5;

            float totalTime = 0.0f;
            Color color;

            foreach (Block block in blocks)
            {
                color = Globals.ColorPalette.DefaultColor;

                if (block.Flipped)
                {
                    color = block.Color;
                    float delay = 2 * delayEach * block.CellCoords[0];
                    tween.InterpolateCallback(block, delay, "set", "visible", true);
                    tween.InterpolateProperty(block, "modulate", Globals.ColorPalette.NoColor, color, modulateTime * 2, Tween.TransitionType.Sine, delay: delay);
                    tween.InterpolateProperty(block, "scale", Zeros, block.Scale, scaleTime * 2, Tween.TransitionType.Sine, delay: delay);
                    totalTime += 2 * delayEach + scaleTime * 2;
                    continue;
                }

                tween.InterpolateCallback(block, delayNext, "set", "visible", true);
                tween.InterpolateProperty(block, "modulate", Globals.ColorPalette.NoColor, color, modulateTime, Tween.TransitionType.Sine, delay: delayNext);
                tween.InterpolateProperty(block, "scale", Zeros, block.Scale, scaleTime, Tween.TransitionType.Sine, delay: delayNext);

                delayNext += delayEach;
                totalTime += delayEach + scaleTime;
            }
            return totalTime;
        }


        public static float ChangePanelSwap(Tween tween, ControlTemplate fromControl, ControlTemplate toControl, float delay = 0.0f)
        {
            float scaleFactor = 1.1f;
            float scaleTime = 0.2f;
            float swapTime = 0.4f;

            float fromPos = fromControl.RectPosition.x;
            float toPos = toControl.RectPosition.x;


            if (fromControl.GetParent() == toControl.GetParent())
            {
                var parent = fromControl.GetParent();
                tween.InterpolateCallback(fromControl, delay, "MoveControl", toControl.GetIndex());
            }

            tween.InterpolateProperty(toControl, "rect_scale", toControl.RectScale, toControl.RectScale / scaleFactor, scaleTime, Tween.TransitionType.Sine);
            tween.InterpolateProperty(toControl, "rect_scale", toControl.RectScale / scaleFactor, toControl.RectScale, scaleTime, Tween.TransitionType.Sine, delay: delay + scaleTime + swapTime);

            tween.InterpolateProperty(fromControl, "rect_scale", fromControl.RectScale, fromControl.RectScale * scaleFactor, scaleTime, Tween.TransitionType.Sine, delay: delay);
            tween.InterpolateProperty(fromControl, "rect_scale", fromControl.RectScale * scaleFactor, fromControl.RectScale, scaleTime, Tween.TransitionType.Sine, delay: delay + scaleTime + swapTime);

            tween.InterpolateProperty(fromControl, "rect_position:x", fromPos, toPos, swapTime, Tween.TransitionType.Sine, delay: delay + scaleTime);
            tween.InterpolateProperty(toControl, "rect_position:x", toPos, fromPos, swapTime, Tween.TransitionType.Sine, delay: delay + scaleTime);

            return delay + 2 * scaleTime + swapTime;
        }

        public static float SlidePanelSwap(Tween tween, ControlTemplate fromControl, ControlTemplate toControl, float delay = 0.0f)
        {
            float scaleFactor = 1.1f;
            float scaleTime = 0.2f;
            float swapTime = 0.4f;

            float fromPos = fromControl.RectPosition.x;
            float toPos = toControl.RectPosition.x;
            Vector2 finalScale = fromControl.RectScale * scaleFactor;
            // int finalLayer = -1;

            if (!fromControl.Active)
            {
                //finalLayer = 1;
                finalScale = fromControl.RectScale / scaleFactor;
            }

            // if (fromControl.GetParent() is CanvasLayer fromLayer && toControl.GetParent() is CanvasLayer toLayer)
            // {
            //     tween.InterpolateCallback(fromLayer, delay, "set", "layer", finalLayer);
            //     tween.InterpolateCallback(fromLayer, 2 * scaleTime + swapTime, "set", "layer", 0);
            // }

            tween.InterpolateProperty(fromControl, "rect_scale", fromControl.RectScale, finalScale, scaleTime, Tween.TransitionType.Sine, delay: delay);
            tween.InterpolateProperty(fromControl, "rect_scale", finalScale, fromControl.RectScale, scaleTime, Tween.TransitionType.Sine, delay: delay + scaleTime + swapTime);

            tween.InterpolateProperty(fromControl, "rect_position:x", fromPos, toPos, swapTime, Tween.TransitionType.Sine, delay: delay + scaleTime);

            return delay + 2 * scaleTime + swapTime;
        }
        public static void Idle(Tween tween, Block block, float finalTime)
        {
            tween.InterpolateCallback(block, finalTime, "SetState", Globals.BLOCKSTATE.IDLE);
        }
        public static void Start(Tween tween, float finalTime, params Block[] blockList)
        {
            foreach (Block block in blockList)
            {
                Idle(tween, block, finalTime);
            }

            tween.Start();
        }
        public static void Start(Tween tween, float finalTime, Godot.Collections.Array<Block> blocks)
        {
            foreach (Block block in blocks)
            {
                Idle(tween, block, finalTime);
            }

            tween.Start();
        }
        public static void Start(Tween tween)
        {
            tween.Start();
        }
    }
}