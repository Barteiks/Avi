using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avi.UI
{
    public sealed class MainPageAnim
    {
        // Breathing: fade in/out forever
        public void StartBreathingAnimation(VisualElement element, double from = 0.0)
        {
            element.Opacity = from;
            // MAUI doesn't have built-in infinite oscillation helper; use a repeating async loop
            _ = BreathingLoop(element);
        }

        bool _breathingCancel;
        async Task BreathingLoop(VisualElement element)
        {
            _breathingCancel = false;
            while (!_breathingCancel)
            {
                await element.FadeTo(1.0, 1500, Easing.SinInOut);
                await element.FadeTo(0.0, 1500, Easing.SinInOut);
            }
            // Ensure returned to visible state
            await element.FadeTo(1.0, 350, Easing.SinOut);
        }

        public void StopBreathingAnim(VisualElement element)
        {
            _breathingCancel = true;
            // ensure visible
            _ = element.FadeTo(1.0, 350, Easing.SinOut);
        }

        // Squash: animate scale Y towards 0 then run afterCompleted
        public async Task SquashElementAsync(VisualElement element, int durationMs = 300, Action? afterCompleted = null)
        {
            // ensure transform origin center — in MAUI we use Scale and ScaleY
            element.AnchorX = 0.5f;
            element.AnchorY = 0.5f;

            // animate scaleY to 0 (simulate squash)
            await element.ScaleTo(1.0, 0); // ensure starting scale
            // MAUI ScaleTo uses uniform scale; to mimic only Y we use a trick with a container or animate Height.
            // Simpler: fade out quickly and collapse height with Scale.
            await element.ScaleTo(0.0, (uint)durationMs, Easing.SinInOut);
            afterCompleted?.Invoke();
            // restore scale back to 1
            await element.ScaleTo(1.0, (uint)Math.Max(150, durationMs), Easing.SinInOut);
        }

        public async Task ShowFaceAsync(VisualElement face)
        {
            // show face with fade-in
            face.Opacity = 0;
            face.IsVisible = true;
            await face.FadeTo(1.0, 300, Easing.SinOut);
        }
    }
}
