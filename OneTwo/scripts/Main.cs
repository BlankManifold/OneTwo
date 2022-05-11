using Godot;

namespace Main
{
    public class Main : Node2D
    {
        private RealGrid _grid;
        private SettingsControl _settingsControl;
        private MainControl _mainControl;
        private BaseTutorial _helpControl;
        private BaseTutorial _tutorialControl;
        private Control _gridControl;
        private Tween _tween;
        private int _highscore = -1;
        private Label _highscoreLabel;

        private AudioManager _audioManager;
        private AudioStreamPlayer _mainAudioPlayer;
        private AnimationPlayer _animationPlayer;
        private GameUI _gameUI;

        private Godot.Collections.Dictionary _settingsDict = new Godot.Collections.Dictionary { { "MusicOn", true }, { "SoundOn", true }, { "Played", false } };


        [Signal]
        delegate void PlayTutorial();

        public override void _Ready()
        {

            GetNode<ColorRect>("BackgroundLayer/ColorRect").Color = Globals.ColorManager.CurrentColorPalette.BackgroundColorMain;
            GetNode<TextureRect>("BackgroundLayer/TextureRect").Modulate = new Color(Globals.ColorManager.CurrentColorPalette.BackgroundColorSecondary, 0.5f);

            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _gridControl = GetNode<Control>("GridLayer/MainControl/GridControl");
            _gameUI = GetNode<GameUI>("GridLayer/MainControl/GameUI");
            _settingsControl = GetNode<SettingsControl>("GridLayer/SettingsControl");
            _mainControl = GetNode<MainControl>("GridLayer/MainControl");
            _helpControl = GetNode<BaseTutorial>("GridLayer/HelpControl");
            _tutorialControl = GetNode<BaseTutorial>("GridLayer/TutorialControl");
            _tween = GetNode<Tween>("MainTween");
            _audioManager = GetNode<AudioManager>("AudioManager");
            _mainAudioPlayer = _audioManager.GetNode<AudioStreamPlayer>("MainAudioPlayer");

            _highscoreLabel = _settingsControl.GetNode<Label>("HighscoreLabel");
            UpdateHighscore();

            _gameUI.DisableButtonsState(true);
            _tutorialControl.DisableButtonsState(true);

            InitSettings();
        }

        private void InitSettings()
        {
            _settingsDict = SaveManager.LoadSettings();

            _audioManager.SoundOn = (bool)_settingsDict["SoundOn"];
            _audioManager.MusicOn = (bool)_settingsDict["MusicOn"];

            _settingsControl.GetNode<TextureButton>("MusicRect/MusicOnButton").Pressed = !_audioManager.MusicOn;
            _settingsControl.GetNode<TextureButton>("SoundRect/SoundOnButton").Pressed = !_audioManager.SoundOn;
        }

        public void _on_GameUI_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "RestartButton":
                    if (_grid.GridState != Globals.GRIDSTATE.GENERATING && _grid.GridState != Globals.GRIDSTATE.WINNING)
                    {
                        if (_grid.GridState == Globals.GRIDSTATE.WIN)
                        {
                            _gameUI.ResetIdleState();
                        }

                        _grid.Restart();
                    }

                    break;

                case "SettingsButton":

                    UpdateHighscore();

                    ChangePanel(_settingsControl, _mainControl);
                    break;

                case "HelpButton":
                    ChangePanel(_helpControl, _mainControl);
                    _helpControl.StartHelpTween();
                    break;
            }
        }
        public void _on_SettingsControl_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "SoundOnButton":
                    _audioManager.SoundOn = !_audioManager.SoundOn;
                    _settingsDict["SoundOn"] = _audioManager.SoundOn;
                    SaveManager.SaveSettings(_settingsDict);

                    break;

                case "MusicOnButton":
                    _audioManager.MusicOn = !_audioManager.MusicOn;

                    _settingsDict["MusicOn"] = _audioManager.MusicOn;
                    SaveManager.SaveSettings(_settingsDict);

                    if (_audioManager.MusicOn)
                    {
                        _mainAudioPlayer.Play();
                    }
                    else
                    {
                        _mainAudioPlayer.Stop();
                    }

                    break;

                case "BackButton":
                    ChangePanel(_mainControl, _settingsControl);
                    break;
            }
        }
        public void _on_HelpControl_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "BackButton":
                    _helpControl.StopHelp();
                    ChangePanel(_mainControl, _helpControl);
                    break;
            }
        }
        public void _on_TutorialControl_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "SkipButton":
                    _tutorialControl.StopHelp();
                    _animationPlayer.Play("PlayFromTutorial");
                    break;
            }
        }
        public async void _on_TitleScreen_button_pressed(string buttonName)
        {
            switch (buttonName)
            {
                case "Play":

                    if ((bool)_settingsDict["MusicOn"])
                    {
                        _tween.InterpolateProperty(_mainAudioPlayer, "volume_db", -40, -10, 4);
                        _tween.Start();
                        _mainAudioPlayer.Play();
                    }

                    if (!SaveManager.AlreadyPlayed())
                    {
                        int sizeConstraint = (int)GetViewport().GetVisibleRect().Size.x - 200;
                        Vector2 cellRatio = new Vector2(1, 1);
                        Vector2 cellSize = new Vector2(64, 64);
                        Vector2 cellBorder = new Vector2(10, 10);
                        Vector2 gridSize = new Vector2(4, 6);

                        _settingsDict["Played"] = true;
                        SaveManager.SaveSettings(_settingsDict);

                        _tutorialControl.InstanceGrid(gridSize, cellSize, cellBorder, cellRatio, sizeConstraint -50);
                        _tutorialControl.DisableButtonsState(false);

                        _animationPlayer.Play("PlayTutorial");
                        await ToSignal(this, nameof(PlayTutorial));
                        
                        _tutorialControl.StartHelpTween(0f);
                    
                        break;
                    }

                    _animationPlayer.Play("Play");
                    break;

            }
        }
        public void _on_Grid_WinState(bool winning)
        {
            _gameUI.SetWinState(winning);
        }

        public void _on_AnimationPlayer_animation_finished(string animation)
        {
            if (animation == "Play" || animation == "PlayFromTutorial")
            {
                _gameUI.DisableButtonsState(false);

                _grid = Globals.PackedScenes.RealGridScene.Instance<RealGrid>();
                InitGridAndHelpGrid();

                if (animation == "Play")
                {
                    GetNode("GridLayer/TitleScreen").CallDeferred("queue_free");
                    return;
                }
                if (animation == "PlayFromTutorial")
                {
                    _tutorialControl.CallDeferred("queue_free");
                    return;
                }
            }

            if (animation == "PlayTutorial")
            {
                EmitSignal(nameof(PlayTutorial));
                GetNode("GridLayer/TitleScreen").CallDeferred("queue_free");
            }
        }


        public void RotateGrid()
        {
            _grid.Rotation = Mathf.Pi;
        }
        public void InitGridAndHelpGrid()
        {
            int sizeConstraint = (int)GetViewport().GetVisibleRect().Size.x - 200;

            if (OS.HasTouchscreenUiHint())
            {
                sizeConstraint = (int)GetViewport().GetVisibleRect().Size.x - 100;
            }

            Vector2 cellRatio = new Vector2(1, 1);
            Vector2 cellSize = new Vector2(64, 64);
            Vector2 cellBorder = new Vector2(10, 10);
            Vector2 gridSize = new Vector2(4, 6);

            _grid.Init(true, gridSize, cellSize * cellRatio, cellBorder, sizeConstraint, true);
            UpdateGridInfo();

            _grid.GridState = Globals.GRIDSTATE.TITLESCREEN;
            _gridControl.AddChild(_grid);

            RotateGrid();
            _helpControl.InstanceGrid(gridSize, cellSize, cellBorder, cellRatio, sizeConstraint - 50);
        }

        private void UpdateGridInfo()
        {
            Globals.GridInfo.UpdateGridInfo(_grid.GridSize, _grid.CellSize, _grid.CellBorder, _grid.Offset);
        }
        private void UpdateHighscore()
        {
            _highscore = SaveManager.LoadHighscore();

            if (_highscore != -1)
            {
                _highscoreLabel.Text = $"HIGHSCORE: {_highscore.ToString("D2")}";

                if (_highscore > 99)
                {
                    _highscoreLabel.Text = $"HIGHSCORE: 99+";
                }
            }
        }



        private async void ChangePanel(ControlTemplate controlIn, ControlTemplate controlOut)
        {
            TweenManager.ChangePanelSwap(_tween, controlOut, controlIn);

            controlOut.Visible = true;
            controlIn.Visible = true;

            controlOut.DisableButtonsState(true);
            controlIn.DisableButtonsState(true);

            TweenManager.Start(_tween);

            await ToSignal(_tween, "tween_all_completed");

            controlOut.UpdateState();
            controlIn.UpdateState();
        }


    }
}