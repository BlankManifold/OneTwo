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

            _musicSlider.SliderRect.Connect("mouse_entered", this, "_on_MusicRect_mouse_entered");
            _soundSlider.SliderRect.Connect("mouse_entered", this, "_on_SoundRect_mouse_entered");
            _musicSlider.SliderRect.Connect("mouse_exited", this, "_on_MusicRect_mouse_exited");
            _soundSlider.SliderRect.Connect("mouse_exited", this, "_on_SoundRect_mouse_exited");
        }

        public override void _Input(InputEvent @event)
        {
            if (_active)
            {
                if (_activeButton)
                {
                    if (@event is InputEventMouseMotion)
                    {
                        _currentSlider.SetRectPosition(_currentSlider.GetLocalMousePosition().x - _clickedPositionX);

                        @event.Dispose();
                        return;
                    }

                    if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == 1)
                    {
                        if (!mouseButton.IsPressed())
                        {
                            _activeButton = false;
                            _currentSlider.ReleasedAction();
                        }
                    }

                    @event.Dispose();
                    return;
                }

                if (_currentSlider != null)
                {
                    if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == 1)
                    {
                        if (mouseButton.IsPressed())
                        {
                            _activeButton = true;
                            _clickedPositionX = _currentSlider.SliderRect.GetLocalMousePosition().x;
                        }

                        mouseButton.Dispose();
                        return;
                    }

                    @event.Dispose();
                    return;
                }

                @event.Dispose();
                return;
            }
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
        public void _on_MusicRect_mouse_entered()
        {
            _currentSlider = _musicSlider;
        }
        public void _on_SoundRect_mouse_entered()
        {
            _currentSlider = _soundSlider;
        }
        public void _on_MusicRect_mouse_exited()
        {
            if (!_activeButton)
                _currentSlider = null;
        }
        public void _on_SoundRect_mouse_exited()
        {
            if (!_activeButton)
                _currentSlider = null;
        }
    }
}