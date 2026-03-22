using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace Avi.face
{
    public class FacePose
    {
        public float LeftBrowX { get; set; } = 0;
        public float LeftBrowY { get; set; } = 0;
        public float RightBrowX { get; set; } = 0;
        public float RightBrowY { get; set; } = 0;
        public float LeftBrowRotation { get; set; } = 0;
        public float RightBrowRotation { get; set; } = 0;
        public float LeftBrowOpacity { get; set; } = 0;
        public float RightBrowOpacity { get; set; } = 0;
        public float LeftEyeScaleY { get; set; } = 1;
        public float RightEyeScaleY { get; set; } = 1;
        public bool LeftEyeEmpty { get; set; } = false;
        public bool RightEyeEmpty { get; set; } = false;
        public bool LeftEyeHappy { get; set; } = false;
        public bool LeftEyeScared { get; set; } = false;
        public bool RightEyeHappy { get; set; } = false;
        public bool RightEyeScared { get; set; } = false;
        public bool LeftBrowCut { get; set; } = false;
        public bool RightBrowCut { get; set; } = false;
    }

    public enum Emotion
    {
        Neutral,
        Confused,
        Happy,
        Scared,
        VeryScared,
        Annoyed,
        Afraid,
        Sad,
        Angry,
        ForcedSmile
    }
}
