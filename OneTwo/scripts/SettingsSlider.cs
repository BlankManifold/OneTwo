using Godot;

namespace Main
{


    public class SettingsSlider : Control
    {
        [Export]
        private Texture _stdTexture;

        [Export]
        private Texture _zeroTexture;


        private TextureRect _sliderRect;
        public TextureRect SliderRect { get { return _sliderRect; } }
        private TextureRect _sliderTextureRect;
        private AudioManager _audioManager;

        [Export]
        private string _type = "";

        private delegate void SliderAction(float value);
        private SliderAction _sliderAction;
        public delegate void ReleasedActionDel();
        private ReleasedActionDel _releasedAction;
        public ReleasedActionDel ReleasedAction { get { return _releasedAction; } }


        private float _offsetX = 0;
        private Vector2 _auxVector;

        public override void _Ready()
        {
            _audioManager = (AudioManager)GetTree().GetNodesInGroup("AudioManager")[0];
            _sliderRect = GetNode<TextureRect>("SliderLine/SliderRect");
            _sliderTextureRect = _sliderRect.GetNode<TextureRect>("SliderTexture");
            _offsetX = _sliderRect.RectSize.x / 2;
            _auxVector.y = _sliderRect.RectPosition.y;

            _sliderRect.SelfModulate = Globals.ColorManager.CurrentColorPalette.ButtonColor;
            _sliderTextureRect.Texture = _stdTexture;
            _releasedAction = delegate () { return; };

            switch (_type)
            {
                case "Sound":
                    _sliderAction = UpdateSound;
                    _releasedAction = TestSound;
                    break;
                case "Music":
                    _sliderAction = UpdateMusic;
                    break;
            }
        }

        public void SetRectPosition(float positionX)
        {
            _auxVector.x = Mathf.Clamp(positionX, -100 - _offsetX, 100 - _offsetX);
            _sliderRect.RectPosition = _auxVector;

            float sliderPositionX = _auxVector.x + _offsetX;
            _sliderAction(sliderPositionX);

            if (sliderPositionX == -100)
            {
                _sliderTextureRect.Texture = _zeroTexture;
            }
            else
            {
                _sliderTextureRect.Texture = _stdTexture;
            }
        }

        public void UpdateSound(float value)
        {
            _audioManager.UpdateSoundDB(ValueToDB(value, AudioManager.SoundMinDB, AudioManager.SoundMaxDB));

            if (value == -100)
                _audioManager.TurnOffSound();
            else
                _audioManager.TurnOffSound(true);

        }
        public void UpdateMusic(float value)
        {
            _audioManager.UpdateMusicDB(ValueToDB(value, AudioManager.MusicMinDB, AudioManager.MusicMaxDB));

            if (value == -100)
                _audioManager.TurnOffMusic();
            else
                _audioManager.TurnOffMusic(true);
        }

        public void TestSound()
        {
            _audioManager.TestSound();
        }

        private float ValueToDB(float value, float minDB, float maxDB)
        {
            return (maxDB - minDB) * value / 200 + (maxDB + minDB) / 2;
        }
        private float DBToValue(float DB, float minDB, float maxDB)
        {
            return (DB - (maxDB + minDB) / 2) * 200 / (maxDB - minDB);
        }

        public void SetRectPositionFromDB(float DB)
        {
            float sliderPositionX = 0;

            if (_type == "Sound")
            {
                sliderPositionX = DBToValue(DB, AudioManager.SoundMinDB, AudioManager.SoundMaxDB);
            }
            else
            {
                sliderPositionX = DBToValue(DB, AudioManager.MusicMinDB, AudioManager.MusicMaxDB);
            }

            _auxVector.x = Mathf.Clamp(sliderPositionX - _offsetX, -100 - _offsetX, 100 - _offsetX);
            _sliderRect.RectPosition = _auxVector;

            if (sliderPositionX == -100)
            {
                _sliderTextureRect.Texture = _zeroTexture;
            }
            else
            {
                _sliderTextureRect.Texture = _stdTexture;
            }

        }

    }

}