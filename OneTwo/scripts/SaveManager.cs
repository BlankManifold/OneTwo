using Godot;


namespace Main
{
    public static class SaveManager
    {
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
                return new Godot.Collections.Dictionary() { { "MusicDB", (AudioManager.MusicMaxDB + AudioManager.MusicMinDB)/2}, { "SoundDB", (AudioManager.SoundMaxDB + AudioManager.SoundMinDB)/2 },
                                                            { "MusicOn", true }, { "SoundOn", true }, { "Played", false}, {"Version", "0.4.1"} };
            }

            Godot.Collections.Dictionary settingsDict = (Godot.Collections.Dictionary)file.GetVar();
            file.Close();
            
            return UpdateFromOldVersion(settingsDict);
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

        public static Godot.Collections.Dictionary UpdateFromOldVersion(Godot.Collections.Dictionary settingsDict)
        {
            
            if (!settingsDict.Contains("Version"))
            {
                settingsDict.Clear();
                settingsDict.Add("Version","0.4.1");
                settingsDict.Add("Played",false);
                settingsDict.Add("MusicDB",(AudioManager.MusicMaxDB + AudioManager.MusicMinDB)/2);
                settingsDict.Add("SoundDB",(AudioManager.SoundMaxDB + AudioManager.SoundMinDB)/2);
                settingsDict.Add("MusicOn",true);
                settingsDict.Add("SoundOn",true);

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