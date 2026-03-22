using SkiaSharp;

namespace Avi.face.Animation
{
    public class FaceAnimator
    {
        readonly Face _face;
        readonly Dictionary<Emotion, FacePose> _poses;

        FacePose? _target;
        FacePose? _pendingTarget;

        readonly BlinkAnimator _blink;

        float dtSmooth = 0.016f;

        // Animation channels
        AnimationChannel eyeL = new();
        AnimationChannel eyeR = new();

        AnimationChannel eyeLH = new();
        AnimationChannel eyeRH = new();

        AnimationChannel eyeLS = new();
        AnimationChannel eyeRS = new();

        AnimationChannelColor eyeLFC = new();
        AnimationChannelColor eyeRFC = new();

        AnimationChannel browLX = new();
        AnimationChannel browLY = new();
        AnimationChannel browLR = new();
        AnimationChannel browLO = new();

        AnimationChannel browRX = new();
        AnimationChannel browRY = new();
        AnimationChannel browRR = new();
        AnimationChannel browRO = new();

        public FaceAnimator(Face face, FaceFadeAnimator fade)
        {
            _face = face;
            _poses = new Emotions().Create();

            _blink = new BlinkAnimator(
                face,
                eyeL, eyeR,
                browLY, browRY,
                browLX, browRX,
                browLR, browRR
            );

            var timer = face.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += (s, e) =>
            {

                fade.Update(dtSmooth);
                Update(dtSmooth);
                
                face.InvalidateSurface();
            };
            timer.Start();
        }
        public void ShowStartupPose()
        {
            
            ApplyEmotion(Emotion.Neutral);
        }
        public void ApplyEmotion(Emotion emotion)
        {
            if (!_poses.TryGetValue(emotion, out var pose))
                return;

            ApplyPose(pose);
        }
        public void ApplyPose(FacePose pose)
        {
            bool needsBlink =
                _target == null ||
                _target.LeftEyeEmpty != pose.LeftEyeEmpty ||
                _target.LeftBrowOpacity != pose.LeftBrowOpacity;

            if (needsBlink)
            {
                _blink.HoldEyesClosed = false;
                _pendingTarget = pose;
                _blink.CloseEye();
                
            }
            else
            {
                SetSmoothPose(pose);
            }

            _target = pose;
        }
        public void ApplyEmotionFromString(string emotionStr)
        {
            if (Enum.TryParse<Emotion>(emotionStr, true, out var emotion))
                ApplyEmotion(emotion);
            else
                ApplyEmotion(Emotion.Neutral);
        }




        void SetSmoothPose(FacePose pose)
        {
            browLX.SetSmooth(pose.LeftBrowX);
            browLY.SetSmooth(pose.LeftBrowY);
            browLR.SetSmooth(pose.LeftBrowRotation);
            browLO.SetSmooth(pose.LeftBrowOpacity);

            browRX.SetSmooth(pose.RightBrowX);
            browRY.SetSmooth(pose.RightBrowY);
            browRR.SetSmooth(pose.RightBrowRotation);
            browRO.SetSmooth(pose.RightBrowOpacity);

            eyeL.SetSmooth(pose.LeftEyeScaleY);

            eyeR.SetSmooth(pose.RightEyeScaleY);

            eyeLH.SetSmooth(pose.LeftEyeHappy ? 100f : 150f);
            eyeRH.SetSmooth(pose.RightEyeHappy ? 100f : 150f);

            eyeLS.SetSmooth(pose.LeftEyeScared ? 1f : 0f);
            eyeRS.SetSmooth(pose.RightEyeScared ? 1f : 0f);

            eyeLFC.SetTween(pose.LeftEyeEmpty ? SKColor.Parse("#00000000") : SKColor.Parse("#FEFFED"));

            eyeRFC.SetTween(pose.RightEyeEmpty ? SKColor.Parse("#00000000") : SKColor.Parse("#FEFFED"));
            
        }

        public void Update(float dt)
        {
            if (_target == null || _face.StartupMode)
                return;

            // Blink end logic
            _blink.Update(dt);
            _blink.BlinkFinished += () =>
            {
                if (_blink.IsBlinking == true && _pendingTarget != null && !_blink.HoldEyesClosed)
                {
                    var p = _pendingTarget;

                    _blink.OpenEye(
                        p.LeftEyeScaleY,
                        p.RightEyeScaleY,
                        p.LeftBrowY,
                        p.RightBrowY
                    );

                    SetSmoothPose(p);
                    _pendingTarget = null;
                }
            };


            // Update channels
            eyeL.Update(dt);
            eyeR.Update(dt);

            eyeLH.Update(dt);
            eyeRH.Update(dt);

            eyeLS.Update(dt);
            eyeRS.Update(dt);

            eyeLFC.Update(dt);
            eyeRFC.Update(dt);

            browLX.Update(dt);
            browLY.Update(dt);
            browLR.Update(dt);
            browLO.Update(dt);

            browRX.Update(dt);
            browRY.Update(dt);
            browRR.Update(dt);
            browRO.Update(dt);

            // Assign to face
            _face.LeftEyeScaleY = eyeL.Current;
            _face.RightEyeScaleY = eyeR.Current;


            _face.LeftEyeAddtionHappy = eyeLH.Current;
            _face.RightEyeAddtionHappy = eyeRH.Current;

            _face.LeftEyeScared = eyeLS.Current;
            _face.RightEyeScared = eyeRS.Current;

            _face.LeftEyeFillColor = eyeLFC.Current;
            _face.RightEyeFillColor = eyeRFC.Current;

            _face.LeftBrowTranslateX = browLX.Current;
            _face.LeftBrowTranslateY = browLY.Current;
            _face.LeftBrowRotation = browLR.Current;
            _face.LeftBrowOpacity = browLO.Current;

            _face.RightBrowTranslateX = browRX.Current;
            _face.RightBrowTranslateY = browRY.Current;
            _face.RightBrowRotation = browRR.Current;
            _face.RightBrowOpacity = browRO.Current;
        }
    }
}