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
                GD.Print("Error: loading player data");
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
                GD.Print("Error: saving player data");
                return;
            }

            file.StoreVar(highscore);
            file.Close();
        }

    }
}