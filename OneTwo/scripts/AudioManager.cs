using Godot;


namespace Main
{
    public class AudioManager : Node
    {
        [Export]
        private AudioStream _testSound;
        private AudioStreamPlayer _audioPlayer0;
        private AudioStreamPlayer _mainMusicPlayer;

        private bool _lastMusicOn = true;
        private bool _musicOn = true;
        public bool MusicOn { get {return _musicOn;} set {_musicOn = value; }}
        private bool _soundOn = true;
        public bool SoundOn { get {return _soundOn;} set {_soundOn = value; }}

        private float _musicDB = 0f;
        public float MusicDB { get {return _musicDB;}}
        private float _soundDB = 0f;
        public float SoundDB { get {return _soundDB;}}

        public static float MusicMinDB = -20f;
        public static float MusicMaxDB = 10f;
        public static float MusicBaseDB = -5f;
        public static float SoundMinDB = -10f;
        public static float SoundMaxDB = 10f;
        public static float SoundBaseDB = -7f;



        public override void _Ready()
        {
            _audioPlayer0 = GetNode<AudioStreamPlayer>("GridAudioPlayer");
            _mainMusicPlayer = (AudioStreamPlayer)GetTree().GetNodesInGroup("MusicPlayer")[0];
            // _mainMusicPlayer.VolumeDb = _musicBaseDB;
        }
        public void PlayAudioEffect(AudioStream stream, int db = 0)
        {
            if (_soundOn)
            {
                AudioStreamPlayer currentPlayer = _audioPlayer0;

                if (_audioPlayer0.Playing)
                {
                    currentPlayer = new AudioStreamPlayer();
                    AddChild(currentPlayer);
                    currentPlayer.Connect("finished", this, "on_currentPlayer_finished", new Godot.Collections.Array { currentPlayer });
                }

                currentPlayer.Stream = stream;
                currentPlayer.VolumeDb = _soundDB + db + SoundBaseDB;
                currentPlayer.Play();
            }
        }
        public void on_currentPlayer_finished(AudioStreamPlayer currentPlayer)
        {
            currentPlayer.QueueFree();
        }

        public void UpdateSoundDB(float volumeDb)
        {
            _soundDB = volumeDb;
        }
        public void UpdateMusicDB(float volumeDb)
        {
            _musicDB = volumeDb;
            _mainMusicPlayer.VolumeDb = _musicDB + MusicBaseDB;
        }

        public void TestSound()
        {
            PlayAudioEffect(_testSound, 0);
        }
        public void TurnOffMusic(bool setOn = false)
        {
            _lastMusicOn = _musicOn;
            _musicOn = setOn;
            if (_musicOn != _lastMusicOn)
            {
                if (_musicOn)
                {
                    _mainMusicPlayer.Play();
                    return;
                }
                 _mainMusicPlayer.Stop();
            }
        }
        public void TurnOffSound(bool setOn = false)
        {
            _soundOn = setOn;
        }

        public void SetUpAudio(Godot.Collections.Dictionary settingsDict)
        {
            _musicOn = (bool)settingsDict["MusicOn"];
            _musicDB = (float)settingsDict["MusicDB"];
            _lastMusicOn = _musicOn;
            _mainMusicPlayer.VolumeDb = _musicDB + MusicBaseDB;

            _soundOn = (bool)settingsDict["SoundOn"];
            _soundDB = (float)settingsDict["SoundDB"];
            
        }

    }


}