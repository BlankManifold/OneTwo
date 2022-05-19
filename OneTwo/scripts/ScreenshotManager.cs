using Godot;

namespace Main
{
    public class ScreenshotManager : Node
    {
        private int _screenshotCounter = 0;

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey key)
            {
                if (!key.Pressed && key.Scancode == (uint)KeyList.S)
                {
                    string screenshot_path = $"user://screenshots/screenshot{_screenshotCounter}.png";
                    Image image = GetTree().Root.GetTexture().GetData();
                    image.FlipY();
                    image.SavePng(screenshot_path);
                    _screenshotCounter++;
                }

                key.Dispose();
                return;
            }

            @event.Dispose();
            return;
        }
    }

}