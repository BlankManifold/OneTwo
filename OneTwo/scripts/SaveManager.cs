using Godot;


namespace Main
{
    public static class SaveManager
    {
        private static Godot.Collections.Dictionary _defaultSettings = new Godot.Collections.Dictionary(){   { "MusicDB", (AudioManager.MusicMaxDB + AudioManager.MusicMinDB)/2},
                                                                                                            { "SoundDB", (AudioManager.SoundMaxDB + AudioManager.SoundMinDB)/2 },
                                                                                                            { "MusicOn", true }, { "SoundOn", true }, { "Played", false}, {"Version", "0.4.1"}
                                                                                                        };
        
        public static int LoadHighscore()
        {
            File file = new File();
            Error err = file.Open("user://highscore.data", File.ModeFlags.Read);
            if (err != 0)
            {
                return -1;
            }

            int highscore = (int)file.GetVar();

            file.Close();

            return highscore;
        }
        public static void SaveHighscore(int highscore)
        {
            File file = new File();
            Error err = file.Open("user://highscore.data", File.ModeFlags.Write);
            if (err != 0)
            {
                return;
            }

            file.StoreVar(highscore);
            file.Close();
        }
        
        public static Godot.Collections.Dictionary LoadSettings()
        {
            File file = new File();
            Error err = file.Open("user://settings.data", File.ModeFlags.Read);
            if (err != 0)
            {
                return _defaultSettings;
            }

            Godot.Collections.Dictionary settingsDict = (Godot.Collections.Dictionary)file.GetVar();
            file.Close();

            return UpdateFromOldVersion(settingsDict);
        }
        public static Godot.Collections.Dictionary LoadStats()
        {
            File file = new File();
            Error err = file.Open("user://stats.data", File.ModeFlags.Read);
            if (err != 0)
            {
                return new Godot.Collections.Dictionary { { "Active", false }, { "GamesPlayed", 0 }, { "GamesFinished", 0 }, { "Best5Mean", 0.0f },  { "BestSingle", 0 } };
            }

            Godot.Collections.Dictionary statsDict = (Godot.Collections.Dictionary)file.GetVar();
            file.Close();

            return statsDict;
        }
        public static Godot.Collections.Dictionary<int,int> LoadMovesDistribution()
        {
            File file = new File();
            Error err = file.Open("user://movesDistribution.data", File.ModeFlags.Read);
            if (err != 0)
            {
                return new Godot.Collections.Dictionary<int,int>() { };
            }

            Godot.Collections.Dictionary<int,int> statsDict = new Godot.Collections.Dictionary<int,int> ((Godot.Collections.Dictionary)file.GetVar());
            file.Close();

            return statsDict;
        }

        public static bool AlreadyPlayed()
        {
            File file = new File();
            Error err = file.Open("user://settings.data", File.ModeFlags.Read);
            if (err != 0)
            {
                return false;
            }

            Godot.Collections.Dictionary settingsDict = (Godot.Collections.Dictionary)file.GetVar();

            file.Close();

            return (bool)settingsDict["Played"];
        }
        public static void SaveSettings(Godot.Collections.Dictionary settingsDict)
        {
            File file = new File();
            Error err = file.Open("user://settings.data", File.ModeFlags.Write);
            if (err != 0)
            {
                return;
            }

            file.StoreVar(settingsDict);
            file.Close();
        }
        
        public static void SaveMovesDistribution(Godot.Collections.Dictionary movesDistributionDict)
        {
            File file = new File();
            Error err = file.Open("user://movesDistribution.data", File.ModeFlags.Write);
            if (err != 0)
            {
                return;
            }

            file.StoreVar(movesDistributionDict);
            file.Close();
        }
        public static void SaveStats(Godot.Collections.Dictionary statsDict)
        {
            File file = new File();
            Error err = file.Open("user://movesDistribution.data", File.ModeFlags.Write);
            if (err != 0)
            {
                return;
            }

            file.StoreVar(statsDict);
            file.Close();
        }

        public static Godot.Collections.Dictionary UpdateFromOldVersion(Godot.Collections.Dictionary settingsDict)
        {

            if (!settingsDict.Contains("Version"))
            {
                settingsDict.Clear();
                settingsDict.Add("Version", "0.4.1");
                settingsDict.Add("Played", false);
                settingsDict.Add("MusicDB", (AudioManager.MusicMaxDB + AudioManager.MusicMinDB) / 2);
                settingsDict.Add("SoundDB", (AudioManager.SoundMaxDB + AudioManager.SoundMinDB) / 2);
                settingsDict.Add("MusicOn", true);
                settingsDict.Add("SoundOn", true);

                SaveSettings(settingsDict);
                return settingsDict;
            }

            if ((string)settingsDict["Version"] == Globals.GameInfo.Version)
            {
                return settingsDict;
            }

            if ((string)settingsDict["Version"] != Globals.GameInfo.Version)
            {
                settingsDict["Version"] = Globals.GameInfo.Version;
                SaveSettings(settingsDict);

                return settingsDict;
            }

            return settingsDict;


        }

    }
}