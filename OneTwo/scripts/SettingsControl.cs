using Godot;

namespace Main
{


    public class SettingsControl : ControlTemplate
    {
        private SettingsSlider _currentSlider;
        private SettingsSlider _musicSlider;
        private SettingsSlider _soundSlider;
        private Vector2 _auxVector;
        private float _clickedPositionX;
        private bool _activeButton = false;

        public override void _Ready()
        {
            base._Ready();

            _musicSlider = GetNode<SettingsSlider>("MusicSlider");
            _soundSlider = GetNode<SettingsSlider>("SoundSlider");

            _musicSlider.SliderRect.Connect("button_down", this, "_on_MusicRect_button_down");
            _soundSlider.SliderRect.Connect("button_down", this, "_on_SoundRect_button_down");
            _musicSlider.SliderRect.Connect("button_up", this, "_on_MusicRect_button_up");
            _soundSlider.SliderRect.Connect("button_up", this, "_on_SoundRect_button_up");
        }

        public override void _Input(InputEvent @event)
        {
            if (_active)
            {
                if (_currentSlider != null)
                {
                    if (@event is InputEventMouseMotion)
                    {
                        _currentSlider.SetRectPosition(_currentSlider.GetLocalMousePosition().x - _clickedPositionX);

                        @event.Dispose();
                        return;
                    }
                }

                @event.Dispose();
                return;
            }

            @event.Dispose();
            return;
        }

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

        public void SetUpAudio(Godot.Collections.Dictionary settingsDict)
        {
            _soundSlider.SetRectPositionFromDB((float)settingsDict["SoundDB"]);
            _musicSlider.SetRectPositionFromDB((float)settingsDict["MusicDB"]);
        }
        public void _on_MusicRect_button_down()
        {
            _currentSlider = _musicSlider;
            _clickedPositionX = _currentSlider.SliderRect.GetLocalMousePosition().x;
        }
        public void _on_SoundRect_button_down()
        {
            _currentSlider = _soundSlider;
            _clickedPositionX = _currentSlider.SliderRect.GetLocalMousePosition().x;
        }
        public void _on_MusicRect_button_up()
        {
            _currentSlider.ReleasedAction();
            _currentSlider = null;
        }
        public void _on_SoundRect_button_up()
        {
            _currentSlider.ReleasedAction();
            _currentSlider = null;
        }
    }
}