using Godot;
using System.Collections.Generic;

namespace Main
{
    public class TutorialControl : BaseTutorial
    {
        private int _readNum = 0;
        private List<bool> _readList = new List<bool>() { false, false, false, false, false };
        public override void _Process(float _)
        {
            if (!_readList[_helpIndex])
            {
                _readList[_helpIndex] = true;
                _readNum += 1;
            }

            if (_readNum == 5)
            {
                _readNum = -1;
                _animationPlayer.Play("SkipButtonDissolve");

                //     _animationPlayer.AnimationSetNext("SkipButtonDissolve", "SkipButtonAppears");
                //    _animationPlayer.AnimationSetNext("SkipButtonAppears","SkipButtonModulate"); 
            }
        }

        public void _on_AnimationPlayer_animation_finished(string name)
        {
            if (name == "SkipButtonDissolve")
            {
                GetNode<Label>("SkipButton/SkipLabel").Text = "PLAY";
                _animationPlayer.Play("SkipButtonAppears");
            }
            
            if (name == "SkipButtonAppears")
            {
                _animationPlayer.Play("SkipButtonModulate");
            }
        }


    }
}