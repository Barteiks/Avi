namespace Avi.face.Animation
{
    public class BlinkAnimator
    {
        public bool HoldEyesClosed { get; set; } = false;

        readonly AnimationChannel eyeL;
        readonly AnimationChannel eyeR;

        readonly AnimationChannel browLY;
        readonly AnimationChannel browRY;

        readonly AnimationChannel browLR;
        readonly AnimationChannel browRR;

        readonly AnimationChannel browLX;
        readonly AnimationChannel browRX;

        readonly Face _face;

        public bool IsBlinking { get; private set; }

        public float BlinkDuration { get; set; } = 0.12f;

        public BlinkAnimator(Face face,
            AnimationChannel eyeL,
            AnimationChannel eyeR,
            AnimationChannel browLY,
            AnimationChannel browRY,
            AnimationChannel browLX,
            AnimationChannel browRX,
            AnimationChannel browLR,
            AnimationChannel browRR
            )
        {
            this.eyeL = eyeL;
            this.eyeR = eyeR;
            this.browLY = browLY;
            this.browRY = browRY;
            this.browLX = browLX;
            this.browRX = browRX;
            this.browLR = browLR;
            this.browRR = browRR;
            _face = face;
        }
        public event Action? BlinkFinished;

        public void CloseEye()
        {
            if (IsBlinking)
                return;

            IsBlinking = true;

            float eyeCenterY = _face.LeftEyeCenterY;

            // CLOSED POSE
            eyeL.SetTween(0f, BlinkDuration);
            eyeR.SetTween(0f, BlinkDuration);

            browLY.SetTween(eyeCenterY - _face.LeftBrowBaseY, BlinkDuration);
            //browRY.SetTween(eyeCenterY - _face.RightBrowBaseY, BlinkDuration);

            browLR.SetTween(0, BlinkDuration);
            browRR.SetTween(0, BlinkDuration);
            browLX.SetTween(0, BlinkDuration);
            browRX.SetTween(0, BlinkDuration);

        }

        public void OpenEye(float openScaleL, float openScaleR, float browY_L, float browY_R)
        {
            // OPEN POSE
            eyeL.SetTween(openScaleL, BlinkDuration);
            eyeR.SetTween(openScaleR, BlinkDuration);

            browLY.SetTween(browY_L, BlinkDuration);
            browRY.SetTween(browY_R, BlinkDuration);

            IsBlinking = false;
        }

        public void Update(float dt)
        {
            bool closingEnded =
                eyeL.Mode == AnimMode.Tween &&
                eyeL.Time >= 1f &&
                IsBlinking &&
                eyeL.Target == 0f;

            bool openingEnded =
                eyeL.Mode == AnimMode.Tween &&
                eyeL.Time >= 1f &&
                !IsBlinking;

            // Jeśli zakończyło się zamykanie → rozpocznij otwieranie
            if (closingEnded)
            {
                // UWAGA: NIE ruszamy docelowej pozycji tutaj. 
                // FaceAnimator sam poda wartości do EndBlink.
                // Tu tylko mówimy że trzeba otworzyć.
                BlinkFinished?.Invoke();
            }
        }


    }
}