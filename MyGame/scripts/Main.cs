using Godot;

namespace Main
{
    public class Main : Node2D
    {

        [Export(PropertyHint.ColorNoAlpha)]
        private Color _defaultColor = Globals.ColorManager.CurrentColorPalette.DefaultColor;

        [Export(PropertyHint.ColorNoAlpha)]
        private Color _offColor = Globals.ColorManager.CurrentColorPalette.OffColor;
        private RealGrid _grid;
        private ControlTemplate _settingsControl;
        private ControlTemplate _mainControl;
        private HelpControl _helpControl;
        private Control _gridControl;
        private Tween _tween;
        private int _highscore = -1;
        private Label _highscoreLabel;

        private AudioManager _audioManager;
        private AudioStreamPlayer _mainAudioPlayer;
        private GameUI _gameUI;

        private Godot.Collections.Dictionary _settingsDict = new Godot.Collections.Dictionary { { "MusicOn", true }, { "SoundOn", true } };

        public override void _Ready()
        {
             
            GetNode<ColorRect>("BackgroundLayer/ColorRect").Color = Globals.ColorManager.CurrentColorPalette.BackgroundColorMain;
            GetNode<TextureRect>("BackgroundLayer/TextureRect").Modulate = new Color(Globals.ColorManager.CurrentColorPalette.BackgroundColorSecondary, 0.5f);
            // _gameUI = GetNode<GameUI>("GridLayer/MainNode/GameUI");
            _gridControl = GetNode<Control>("GridLayer/MainControl/GridControl");
            _gameUI = GetNode<GameUI>("GridLayer/MainControl/GameUI");
            _settingsControl = GetNode<ControlTemplate>("GridLayer/SettingsControl");
            _mainControl = GetNode<ControlTemplate>("GridLayer/MainControl");
            _helpControl = GetNode<HelpControl>("GridLayer/HelpControl");
            _tween = GetNode<Tween>("MainTween");
            _audioManager = GetNode<AudioManager>("AudioManager");
            _mainAudioPlayer = _audioManager.GetNode<AudioStreamPlayer>("MainAudioPlayer");

            _highscoreLabel = _settingsControl.GetNode<Label>("HighscoreLabel");
            UpdateHighscore();
             
            InitSettings();
             

            PackedScene gridScene = (PackedScene)ResourceLoader.Load("res://scene/RealGrid.tscn");
            //_grid = Globals.PackedScenes.GridScene.Instance<Grid>();
            _grid = gridScene.Instance<RealGrid>();

             

            int sizeConstraint = (int)GetViewport().GetVisibleRect().Size.x - 100;

            if (!OS.HasTouchscreenUiHint())
            {
                sizeConstraint = (int)GetViewport().Size.x - 100;
            }

             

            Vector2 cellRatio = new Vector2(1, 1);
            Vector2 cellSize = new Vector2(64, 64);
            Vector2 border =  new Vector2(10, 10);
            Vector2 gridSize = new Vector2(4,6);
            _grid.Init(true, gridSize, cellSize * cellRatio, border, sizeConstraint);
             
            UpdateGridInfo();

             

            RotateGrid();

             

            _gridControl.AddChild(_grid);

             

            _helpControl.InstanceGrid(gridSize, cellSize, border, cellRatio, sizeConstraint - 50);

             

        }

        private void InitSettings()
        {
            _settingsDict = SaveManager.LoadSettings();

            _audioManager.SoundOn = (bool)_settingsDict["SoundOn"];
            _audioManager.MusicOn = (bool)_settingsDict["MusicOn"];

            if ((bool)_settingsDict["MusicOn"])
            {
                _mainAudioPlayer.Play();
            }

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
        public void _on_Grid_WinState(bool winning)
        {
            _gameUI.SetWinState(winning);
        }


        public void RotateGrid()
        {
            _grid.Rotation = Mathf.Pi;
             
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

            TweenManager.Start(_tween);

            await ToSignal(_tween, "tween_all_completed");

            controlOut.UpdateState();
            controlIn.UpdateState();
        }

        public void _on_ColorPicker_color_changed(Color color)
        {
            Globals.ColorManager.CurrentColorPalette.DefaultColor = color;
        }


    }
}