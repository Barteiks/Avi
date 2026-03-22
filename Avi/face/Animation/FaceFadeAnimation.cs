using System;
using System.Collections.Generic;
using System.Text;

namespace Avi.face.Animation
{

    public class FaceFadeAnimator
    {
        private readonly Face _face;
        private readonly AnimationChannel _textOpacity = new();
        private readonly AnimationChannel _faceOpacity = new();

        public FaceFadeAnimator(Face face)
        {
            _face = face;

            // start values
            _textOpacity.Current = 1f;
            _faceOpacity.Current = face.EyeOpacity;
        }

        public void Update(float dt)
        {
            _textOpacity.Update(dt);
            _faceOpacity.Update(dt);
            
            //_face.TextOpacity = _textOpacity.Current;
            _face.EyeOpacity = _faceOpacity.Current;
            _face.Update(dt);
        }


        public Task FadeTextOutAndShowFaceAsync(float duration = 0.6f)
        {
            var tcs = new TaskCompletionSource();

            _textOpacity.SetTween(0f, duration);  // tekst 1 → 0
            _faceOpacity.SetTween(1f, duration);  // twarz 0 → 1

            // po animacji
            Task.Run(async () =>
            {
                await Task.Delay((int)(duration * 1000));
                tcs.SetResult();
            });

            return tcs.Task;
        }
    }

}
