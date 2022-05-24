using Godot;
using System.Collections.Generic;



namespace Main
{
    public class StatsManager : Node
    {
        private Godot.Collections.Dictionary<int, int> _movesDistribution = new Godot.Collections.Dictionary<int, int> { };
        private Godot.Collections.Dictionary _statsDict = new Godot.Collections.Dictionary { { "Active", false }, { "GamesPlayed", 0 }, { "GamesFinished", 0 }, { "Best5Mean", 0.0f },  { "BestSingle", 0 } };
        private float _currentMovesMean5 = 0.0f;
        private int _currentBestSingle = 0;
        private List<int> _last5MovesList = new List<int> { };
        private bool _active = true;

        [Signal]
        delegate void UpdatingStats(Godot.Collections.Dictionary statsDict, Godot.Collections.Dictionary movesDistributionDict);

        public override void _Ready()
        {
            base._Ready();
            Connect(nameof(UpdatingStats), (Main)GetTree().GetNodesInGroup("Main")[0], "_on_StatsManager_UpdatingStats");
        }

        public void LoadStats()
        {
            _movesDistribution = SaveManager.LoadMovesDistribution();
            _statsDict = SaveManager.LoadStats();
            
            EmitSignal(nameof(UpdatingStats), _statsDict, _movesDistribution);
        }

        public void UpdateStats(int moves)
        {
            if (_active)
            {
                UpdateBasicStats(moves);
                UpdateCurrentMovesList(moves);
                UpdateBestSingle(moves);
                UpdateBestMovesMean5();
                UpdateMovesDictionary(moves);
                EmitSignal(nameof(UpdatingStats), _statsDict, _movesDistribution);
            }
        }

        private void UpdateBasicStats(int moves)
        {
            int numberGamePlayed = (int)_statsDict["GamesPlayed"];
            _statsDict["GamesPlayed"] = numberGamePlayed + 1;

            if (moves != -1)
            {
                int numberGameFinished = (int)_statsDict["GamesFinished"];
                _statsDict["GamesFinished"] = numberGameFinished + 1;
            }
        }
        private void UpdateMovesDictionary(int moves)
        {
            if (_movesDistribution.ContainsKey(moves))
            {
                _movesDistribution[moves] += 1;
                return;
            }

            _movesDistribution.Add(moves, 1);
        }
        private void UpdateBestMovesMean5()
        {
            if (_currentMovesMean5 == 0)
            {
                return;
            }

            float bestMovesMean5 =  (float)_statsDict["Best5Mean"];
            if (_currentMovesMean5 < bestMovesMean5 || bestMovesMean5 == 0)
            {
                _statsDict["Best5Mean"] = _currentMovesMean5;
            }
        }
       
        private void UpdateBestSingle(int moves)
        {
            if (moves == -1)
            {
                return;
            }

            int bestSingle =  (int)_statsDict["BestSingle"];
            if (moves < bestSingle || bestSingle == 0)
            {
                _statsDict["BestSingle"] = moves;
            }
        }
        private void UpdateCurrentMovesList(int moves)
        {
            _last5MovesList.Add(moves);

            if (_last5MovesList.Count == 5)
            {
                SetAverageMoves5();
                return;
            }

            if (_last5MovesList.Count == 6)
            {
                _last5MovesList.RemoveAt(0);
                SetAverageMoves5();
                return;
            }

            return;

        }
        private void SetAverageMoves5()
        {
            int sum = 0;
            int min = _last5MovesList[0];
            int max = _last5MovesList[0];
            int dnfs = 0;

            foreach (int move in _last5MovesList)
            {
                sum += move;

                if (move == -1)
                {
                    min = -1;
                    dnfs++;
                    continue;
                }

                if (move > max)
                {
                    max = move;
                }

                if (move < min)
                {
                    min = move;
                }
            }

            if (dnfs > 1)
            {
                _currentMovesMean5 = -1;
                return;
            }


            _currentMovesMean5 = (sum - min - max) / 3.0f;
            return;
        }


    } 
}