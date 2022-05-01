using Godot;


namespace Main
{
    public class AudioManager : Node
    {
        private AudioStreamPlayer _audioPlayer0;

        private bool _musicOn = true;
        public bool MusicOn { get {return _musicOn;} set {_musicOn = value; }}
        private bool _soundOn = true;
        public bool SoundOn { get {return _soundOn;} set {_soundOn = value; }}


        public override void _Ready()
        {
            _audioPlayer0 = GetNode<AudioStreamPlayer>("GridAudioPlayer");
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
                currentPlayer.VolumeDb = db;
                currentPlayer.Play();
            }
        }
        public void on_currentPlayer_finished(AudioStreamPlayer currentPlayer)
        {
            currentPlayer.QueueFree();
        }
    }


}