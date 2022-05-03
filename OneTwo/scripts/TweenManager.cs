using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Main
{
    public static class TweenManager
    {
        public static Vector2 Zeros = Vector2.Zero;

        public static float ScaleFactor = 2.5f;

        public static float SelectModulate(Tween tween, FakeGrid grid, Block block, float delay)
        {
            float modulateTime = 0.2f;
            tween.InterpolateProperty(block, "modulate", Globals.ColorManager.CurrentColorPalette.DefaultColor, block.Color, modulateTime, Tween.TransitionType.Sine, delay: delay);
            tween.InterpolateCallback(grid.AudioManager, delay, "PlayAudioEffect", grid.GetAudioStream("Select"), grid._soundSelectDB);

            return modulateTime + delay;
        }
        public static float UnSelectModulate(Tween tween, Block block, float delay)
        {
            float modulateTime = 0.2f;
            float totalDelay = delay;
            tween.InterpolateProperty(block, "modulate", block.Color, Globals.ColorManager.CurrentColorPalette.DefaultColor, modulateTime, Tween.TransitionType.Sine, delay: totalDelay);

            return modulateTime + totalDelay;
        }
        public static float UnSelectModulate(Tween tween, Block block1, Block block2, float delay)
        {
            float modulateTime = 0.2f;
            float totalDelay = delay + 0.2f;
            tween.InterpolateProperty(block1, "modulate", block1.Color, Globals.ColorManager.CurrentColorPalette.DefaultColor, modulateTime, Tween.TransitionType.Sine, delay: totalDelay);
            tween.InterpolateProperty(block2, "modulate", block2.Color, Globals.ColorManager.CurrentColorPalette.DefaultColor, modulateTime, Tween.TransitionType.Sine, delay: totalDelay);

            return modulateTime + totalDelay;
        }
        public static float SwitchOffScaleModulate(Tween tween, FakeGrid grid, Block block1, Block block2, float delay, bool toBeRepeated = false)
        {
            float scaleFactor = ScaleFactor;
            float scaleTime = 0.8f;
            float modulateTime = 0.8f;
            float flashModulateTime = 0.7f;
            float flashScaleTime = 1.0f;
            float flashScaleFactor = 4f;
            float overlappingTime = 0.3f;


            float totalTime = delay + scaleTime + flashScaleTime - overlappingTime;

            Block block1Ghost = grid.BlockScene.Instance<Block>();
            Block block2Ghost = grid.BlockScene.Instance<Block>();

            block1Ghost.Copy(block1);
            block2Ghost.Copy(block2);
            block1Ghost.Visible = false;
            block2Ghost.Visible = false;
            block1Ghost.ZIndex = 1;
            block2Ghost.ZIndex = 1;

            var parent = tween.GetParent();

            parent.AddChild(block1Ghost);
            parent.AddChild(block2Ghost);

            grid.AddAuxBlock(block1Ghost);
            grid.AddAuxBlock(block2Ghost);

            tween.InterpolateCallback(block1Ghost, delay, "set", "visible", true);
            tween.InterpolateCallback(block2Ghost, delay, "set", "visible", true);


            tween.InterpolateProperty(block1Ghost, "scale", block1Ghost.Scale, block1Ghost.Scale * flashScaleFactor, flashScaleTime, Tween.TransitionType.Sine, delay: delay);
            tween.InterpolateProperty(block2Ghost, "scale", block2Ghost.Scale, block2Ghost.Scale * flashScaleFactor, flashScaleTime, Tween.TransitionType.Sine, delay: delay);

            tween.InterpolateProperty(block1Ghost, "modulate", block1Ghost.Color, Globals.ColorManager.CurrentColorPalette.NoColor, flashModulateTime, Tween.TransitionType.Linear, delay: delay);
            tween.InterpolateProperty(block2Ghost, "modulate", block2Ghost.Color, Globals.ColorManager.CurrentColorPalette.NoColor, flashModulateTime, Tween.TransitionType.Linear, delay: delay);

            tween.InterpolateProperty(block1, "scale", block1.Scale, block1.Scale / scaleFactor, scaleTime, Tween.TransitionType.Sine, delay: delay + flashScaleTime - overlappingTime);
            tween.InterpolateProperty(block2, "scale", block2.Scale, block2.Scale / scaleFactor, scaleTime, Tween.TransitionType.Sine, delay: delay + flashScaleTime - overlappingTime);

            if (block1.CellCoords[1] != 0)
                tween.InterpolateProperty(block1, "modulate", block1.Color, Globals.ColorManager.CurrentColorPalette.OffColor, modulateTime, Tween.TransitionType.Linear, delay: delay + flashScaleTime - overlappingTime);

            if (block2.CellCoords[1] != 0)
                tween.InterpolateProperty(block2, "modulate", block2.Color, Globals.ColorManager.CurrentColorPalette.OffColor, modulateTime, Tween.TransitionType.Linear, delay: delay + flashScaleTime - overlappingTime);

            tween.InterpolateCallback(grid.AudioManager,  delay, "PlayAudioEffect", grid.GetAudioStream("SwitchOff"), grid._soundSwitchOffDB);


            if (toBeRepeated)
            {
                return totalTime;
            }

            tween.InterpolateCallback(block2Ghost, flashScaleTime + delay, "queue_free");
            tween.InterpolateCallback(block1Ghost, flashScaleTime + delay, "queue_free");

            return totalTime;
        }
        public static float SwapHovering(Tween tween, FakeGrid grid, Block fromBlock, Block toBlock, bool tobeScaled, bool diagonalSwap, float delay, float timeFactor = 1.0f, bool toBeRepeated = false)
        {

            float scaleFactor = 1.3f;
            float scaleTime = 0.0f;
            float swapTime = 0.2f * timeFactor;

            Vector2 fromPos = Globals.Utilities.CellCoordsToPosition(toBlock.CellCoords, grid);
            Vector2 toPos = Globals.Utilities.CellCoordsToPosition(fromBlock.CellCoords, grid);

            if (diagonalSwap)
            {
                scaleTime *= Globals.GridInfo.DiagFactor;
                swapTime *= Globals.GridInfo.DiagFactor;
            }

            if (tobeScaled)
            {
                scaleTime = 0.1f * timeFactor;

                if (!fromBlock.IsOff)
                {
                    tween.InterpolateProperty(fromBlock, "scale", fromBlock.Scale, fromBlock.Scale * scaleFactor, scaleTime, Tween.TransitionType.Sine, delay: delay);
                    tween.InterpolateProperty(fromBlock, "scale", fromBlock.Scale * scaleFactor, fromBlock.Scale, scaleTime, Tween.TransitionType.Sine, delay: delay + scaleTime + swapTime);
                    if (!toBeRepeated)
                        tween.InterpolateCallback(fromBlock, 2 * scaleTime + swapTime, "set", "z_index", 0);
                }

                if (!toBlock.IsOff)
                {
                    if (fromBlock.IsOff)
                    {
                        scaleFactor = 1 / scaleFactor;
                    }
                    tween.InterpolateProperty(toBlock, "scale", toBlock.Scale, toBlock.Scale / scaleFactor, scaleTime, Tween.TransitionType.Sine, delay: delay);
                    tween.InterpolateProperty(toBlock, "scale", toBlock.Scale / scaleFactor, toBlock.Scale, scaleTime, Tween.TransitionType.Sine, delay: delay + scaleTime + swapTime);
                    if (!toBeRepeated)
                        tween.InterpolateCallback(toBlock, 2 * scaleTime + swapTime, "set", "z_index", 0);
                }
            }

            tween.InterpolateProperty(fromBlock, "position", fromPos, toPos, swapTime, Tween.TransitionType.Sine, delay: delay + scaleTime);
            tween.InterpolateProperty(toBlock, "position", toPos, fromPos, swapTime, Tween.TransitionType.Sine, delay: delay + scaleTime);

            tween.InterpolateCallback(grid.AudioManager,  delay + 0.2f, "PlayAudioEffect", grid.GetAudioStream("Swap"), grid._soundSwapDB);

            return delay + 2 * scaleTime + swapTime;
        }

        public static float GenerateBlocks(Tween tween, Godot.Collections.Array<Block> blocks)
        {
 
           
            blocks = new Godot.Collections.Array<Block>(blocks.OrderBy(item => Globals.RandomManager.rnd.Next()).ToArray<Block>());
           // blocks.Shuffle();
            float scaleTime = 0.2f;
            float modulateTime = 0.2f;
            float delayEach = 0.1f;
            float delayNext = (2 * scaleTime - 2 * delayEach) * 5;

            float totalTime = 0.0f;
            Color color;
            
 

            foreach (Block block in blocks)
            {
                color = Globals.ColorManager.CurrentColorPalette.DefaultColor;

                if (block.Flipped)
                {
                    color = block.Color;
                    float delay = 2 * delayEach * block.CellCoords[0];
                    tween.InterpolateCallback(block, delay, "set", "visible", true);
                    tween.InterpolateProperty(block, "modulate", Globals.ColorManager.CurrentColorPalette.NoColor, color, modulateTime * 2, Tween.TransitionType.Sine, delay: delay);
                    tween.InterpolateProperty(block, "scale", Zeros, block.Scale, scaleTime * 2, Tween.TransitionType.Sine, delay: delay);
                    totalTime += 2 * delayEach + scaleTime * 2;
                    continue;
                }

                tween.InterpolateCallback(block, delayNext, "set", "visible", true);
                tween.InterpolateProperty(block, "modulate", Globals.ColorManager.CurrentColorPalette.NoColor, color, modulateTime, Tween.TransitionType.Sine, delay: delayNext);
                tween.InterpolateProperty(block, "scale", Zeros, block.Scale, scaleTime, Tween.TransitionType.Sine, delay: delayNext);

                delayNext += delayEach;
                totalTime += delayEach + scaleTime;
            }
 
            return totalTime;
        }

        public static float Win(Tween tween, RealGrid grid, float delay)
        {
            float scaleFactor = ScaleFactor;
            float scaleTime = 0.2f;
            float modulateTime = 0.2f;
            float delayEach = 0.1f;

            List<Block> blocks = grid.GetRow(3);
            List<Color> colors = Globals.ColorManager.GetRandomizeColorList(); 
            Godot.Collections.Array<Label> winLabels = new Godot.Collections.Array<Label>(grid.WinLabel.GetChildren());

            float scaleTotalTime =  blocks.Count*delayEach + scaleTime;
            float totalTime = delay + scaleTotalTime;
            float currentDelay;
            for (int i = 0; i < blocks.Count; i++)
            {
                currentDelay = (blocks.Count-i-1)*delayEach;
                Block block = blocks[i];
                tween.InterpolateProperty(block, "scale", block.Scale, block.OriginalScale, scaleTime, Tween.TransitionType.Sine, delay: delay + currentDelay );
                tween.InterpolateProperty(block, "modulate", Globals.ColorManager.CurrentColorPalette.OffColor, colors[i], modulateTime, Tween.TransitionType.Sine, delay: currentDelay + delay);
                tween.InterpolateCallback(winLabels[blocks.Count-i-1], currentDelay + delay + modulateTime, "set", "visible", true);
            }

             tween.InterpolateCallback(grid.AudioManager, scaleTotalTime +  delay - delayEach - modulateTime, "PlayAudioEffect", grid.GetAudioStream("Win"), grid._soundWinDB);

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


        public static float Help4x6Tip0(FakeGrid grid, Tween tween, float delay = 0.0f, bool toBeRepeateded = true)
        {
            float totalTime = 0.0f;
            float selectDelay = 0.5f;
            float secondSelectDelay = 1.0f;

            Block block1 = grid.GetBlock(1, 2);
            Block block2 = grid.GetBlock(2, 2);
            Block block3 = grid.GetBlock(2, 3);

            totalTime = SelectModulate(tween, grid, block1, delay);
            totalTime = SelectModulate(tween, grid, block2, totalTime + selectDelay);

            totalTime = UnSelectModulate(tween, block1, block2, totalTime + 0.2f);

            totalTime = SelectModulate(tween, grid, block1, totalTime + secondSelectDelay);
            totalTime = SelectModulate(tween, grid, block3, totalTime + selectDelay);

            totalTime = SwitchOffScaleModulate(tween, grid, block1, block3, totalTime, true);

            if (toBeRepeateded)
                return totalTime + 1.0f;
            return totalTime;
        }
        public static float Help4x6Tip1(FakeGrid grid, Tween tween, float delay = 0.0f, bool toBeRepeateded = true)
        {
            float totalTime = 0.0f;
            float selectDelay = 0.5f;
            float secondSelectDelay = 1.0f;

            Block block1 = grid.GetBlock(1, 2);
            Block block2 = grid.GetBlock(2, 2);
            Block block3 = grid.GetBlock(2, 3);


            block1.Swap(block2, true);

            block1.ZIndex = 1;
            block2.ZIndex = -1;
            block3.ZIndex = -2;

            totalTime = SwapHovering(tween, grid, block2, block1, true, false, delay, 1.3f, true);

            totalTime = SelectModulate(tween, grid, block2, totalTime + secondSelectDelay);

            block2.Swap(block3, true);
            totalTime = SwapHovering(tween, grid, block2, block3, true, true, totalTime + selectDelay, 1.3f, true);

            totalTime = UnSelectModulate(tween, block2, totalTime + 0.1f);

            if (toBeRepeateded)
                return totalTime + 1.0f;
            return totalTime;
        }
        public static float Help4x6Tip2(FakeGrid grid, Tween tween, float delay = 0.0f, bool toBeRepeateded = true)
        {
           
            float totalTime = 0.0f;
            float selectDelay = 0.5f;
            float secondSelectDelay = 1.0f;

            Block block1 = grid.GetBlock(1, 2);
            Block block2 = grid.GetBlock(2, 2);
            Block block3 = grid.GetBlock(2, 3);


            block1.Swap(block2, true);

            block1.ZIndex = 1;
            block2.ZIndex = -1;
            block3.ZIndex = -2;

            totalTime = SwapHovering(tween, grid, block2, block1, false, false, delay, 1.3f, true);

            totalTime = SelectModulate(tween, grid, block3, totalTime + secondSelectDelay);

            block3.Swap(block2, true);
            totalTime = SwapHovering(tween, grid, block3, block2, true, true, totalTime + selectDelay, 1.3f, true);

            totalTime = UnSelectModulate(tween, block3, totalTime + 0.1f);

            if (toBeRepeateded)
                return totalTime + 1.0f;
            return totalTime;
        }
        public static float Help4x6Tip3(FakeGrid grid, Tween tween, float delay = 0.0f, bool toBeRepeateded = true)
        {
            float totalTime = 0.0f;
            float selectDelay = 0.5f;
            float secondSelectDelay = 1.0f;

            Block block1 = grid.GetBlock(1, 2);
            Block block3 = grid.GetBlock(2, 3);
            Block block2 = grid.GetBlock(1, 1);
            Block targetBlock = grid.GetBlock(1, 0);



            totalTime = SelectModulate(tween, grid, block1, totalTime + secondSelectDelay);
            totalTime = SelectModulate(tween, grid, block3, totalTime + selectDelay);

            totalTime = SwitchOffScaleModulate(tween, grid, block1, block3, totalTime, true);

            totalTime = SelectModulate(tween, grid, block2, totalTime + secondSelectDelay);
            totalTime = SwitchOffScaleModulate(tween, grid, block2, targetBlock, totalTime, true);

            if (toBeRepeateded)
                return totalTime + 1.0f;
            return totalTime;
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