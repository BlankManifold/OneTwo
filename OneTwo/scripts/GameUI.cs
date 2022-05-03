using Godot;

namespace Main
{
    public class GameUI : ControlTemplate
    {
        private Label _movesLabel;
        private TextureButton _restartButton;
        private TextureButton _settingsButton;
        private TextureButton _helpButton;
        private AnimationPlayer _animationPlayer;

        public override void _Ready()
        {
            base._Ready();

            _movesLabel = GetNode<Label>("MovesLabel");
            _movesLabel.Text = $"MOVES: 00";

            _settingsButton = GetNode<TextureButton>("SettingsButton");
            _restartButton = GetNode<TextureButton>("RestartButton");
            _helpButton = GetNode<TextureButton>("HelpButton");

            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        }

        public void _on_Grid_UpdateMoves(int moves)
        {
            if (moves < 100)
            {
                _movesLabel.Text = $"MOVES: {moves.ToString("D2")}";
                return;
            }

            _movesLabel.Text = $"MOVES: U BAD";
            return;
        }

        public void ResetIdleState()
        {
            _settingsButton.Disabled = false;
            _helpButton.Disabled = false;
            _animationPlayer.Stop();
            _animationPlayer.Play("RESET");
        }

        public void SetWinState(bool winning)
        {
            if (winning)
            {
                _settingsButton.Disabled = true;
                _helpButton.Disabled = true;
                return;
            }
            
            _animationPlayer.Play("WinState");
        }
       
        public void DisableButtonsState(bool disabled)
        {
                _settingsButton.Disabled = disabled;
                _helpButton.Disabled = disabled;
                _restartButton.Disabled = disabled;
        }
    }
}