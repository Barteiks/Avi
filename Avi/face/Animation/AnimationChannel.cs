using SkiaSharp;

namespace Avi.face.Animation
{
    public enum AnimMode
    {
        Smooth,     // exponential smoothing
        Tween       // fixed-time interpolation
    }

    public class AnimationChannel
    {
        public AnimMode Mode = AnimMode.Smooth;

        public float Speed = 0.15f;   // for Smooth
        public float Duration = 0.12f; // for Tween

        public float Current;
        public float Target;
        public float Start;
        public float Time;

        public void Update(float dt)
        {
            if (Mode == AnimMode.Smooth)
            {
                Current += (Target - Current) * Speed;
            }
            else if (Mode == AnimMode.Tween)
            {
                Time += dt / Duration;
                if (Time > 1f) Time = 1f;

                // Ease-in-out
                float t = Time;
                float p = t * t * (3 - 2 * t);

                Current = Start + (Target - Start) * p;
            }
        }

        public void SetSmooth(float target)
        {
            Target = target;
            Mode = AnimMode.Smooth;
        }

        public void SetTween(float target, float duration = 0.12f)
        {
            Start = Current;
            Target = target;
            Duration = duration;
            Time = 0f;
            Mode = AnimMode.Tween;
        }
    }

    public class AnimationChannelColor
    {
        public SKColor Current;
        public SKColor Start;
        public SKColor Target;

        public float Time;
        public float Duration = 0.05f;

        public bool IsTweening => Time < 1f;

        public void SetTween(SKColor target, float duration = 0.05f)
        {
            Start = Current;
            Target = target;
            Duration = duration;
            Time = 0f;
        }

        public void Update(float dt)
        {
            if (Time >= 1f)
                return;

            Time += dt / Duration;
            if (Time > 1f) Time = 1f;

            // standard ease-in-out (jak w AnimationChannel)
            float t = Time;
            float p = t * t * (3 - 2 * t);

            byte r = (byte)(Start.Red + (Target.Red - Start.Red) * p);
            byte g = (byte)(Start.Green + (Target.Green - Start.Green) * p);
            byte b = (byte)(Start.Blue + (Target.Blue - Start.Blue) * p);
            byte a = (byte)(Start.Alpha + (Target.Alpha - Start.Alpha) * p);

            Current = new SKColor(r, g, b, a);
        }
    }
}