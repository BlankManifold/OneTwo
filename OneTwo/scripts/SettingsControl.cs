using Godot;

namespace Main
{

    public class SettingsControl : ControlTemplate
    {
       public override void DisableButtonsState(bool disabled)
        {
            foreach (TextureButton button in _localButtons)
            {
                if (button.Name == "MusicOnButton" || button.Name == "SoundOnButton")
                {
                    continue;
                }

                button.Disabled = disabled;
            }
        }

    }
}