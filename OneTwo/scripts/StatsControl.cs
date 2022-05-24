using Godot;

namespace Main
{
    public class StatsControl : ControlTemplate
    {
        private Label _bestMovesMean5Label;
        private Label _bestSingleLabel;
        private Label _gamesPlayed;
        private Label _gamesFinished;

        public override void _Ready()
        {
            base._Ready();

            _bestMovesMean5Label = GetNode<Label>("BestMovesMean5");
            _bestSingleLabel = GetNode<Label>("BestSingle");
            _gamesPlayed = GetNode<Label>("GamesPlayed");
            _gamesFinished = GetNode<Label>("GamesFinished");
        }

        public void UpdateStats(Godot.Collections.Dictionary statsDict, Godot.Collections.Dictionary movesDistributionDict)
        {
            _bestMovesMean5Label.Text = $"Best mean of 5: {statsDict["Best5Mean"]}";
            _bestSingleLabel.Text = $"Best single: {statsDict["BestSingle"]}";
            _gamesPlayed.Text = $"Games played: {statsDict["GamesPlayed"]}";
            _gamesFinished.Text = $"Games finished: {statsDict["GamesFinished"]}";
        }
        
    }
}