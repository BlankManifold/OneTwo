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
                return new Godot.Collections.Dictionary() { { "MusicOn", true }, { "SoundOn", true }, { "Played", false} };
            }

            Godot.Collections.Dictionary settingsDict = (Godot.Collections.Dictionary)file.GetVar();

            file.Close();

            return settingsDict;
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

    }
}