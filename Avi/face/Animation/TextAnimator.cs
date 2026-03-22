using SkiaSharp;
using System;

namespace Avi.face.Animation
{
    public class FaceTextController
    {
        public SKRect MainTextRect => mainTextRect;
        public string MainText { get; set; } = "Sleep mode";
        public string SubText { get; set; } = "";

        public event Action? MainTextClicked;
        public event Action? SubTextClicked;

        public bool FreezeBreathing { get; set; } = false;
        public float currentOpacity = 1f;
        public float targetOpacity = 1f;
        public float fadeSpeed = 1f;

        private readonly SKColor faceFill;
        private readonly SKColor glowColor;
        private SKRect mainTextRect;
        private SKRect subTextRect;
        private float time = 0f;

        // Cached rendering objects for performance
        private readonly SKFont cachedFont;
        private readonly SKPaint textPaint;
        private readonly SKPaint glowPaint;

        public FaceTextController(SKColor faceFill, SKColor glowColor)
        {
            this.faceFill = faceFill;
            this.glowColor = glowColor;

            cachedFont = new SKFont();
            textPaint = new SKPaint { IsAntialias = true };
            glowPaint = new SKPaint
            {
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 10, 10, glowColor)
            };
        }

        public void Update(float dt)
        {
            if (!FreezeBreathing)
            {
                time += dt;
                currentOpacity = 0.65f + 0.35f * (float)Math.Sin(time * 3f);
            }
            else
            {
                if (Math.Abs(currentOpacity - targetOpacity) > 0.001f)
                {
                    // Obliczamy kierunek (1 dla rozjaśniania, -1 dla znikania)
                    float direction = Math.Sign(targetOpacity - currentOpacity);

                    // Zmieniamy opacity o stałą wartość (fadeSpeed na sekundę)
                    currentOpacity += direction * fadeSpeed * dt;

                    // Zabezpieczenie przed "przestrzeleniem" celu (Clamping)
                    if (direction > 0 && currentOpacity > targetOpacity) currentOpacity = targetOpacity;
                    if (direction < 0 && currentOpacity < targetOpacity) currentOpacity = targetOpacity;
                }
            }
        }

        public void Draw(SKCanvas canvas, float centerX, float mainY, float mainSize, float subY, float subSize)
        {
            DrawTextWithGlow(canvas, MainText, centerX, mainY, mainSize, ref mainTextRect);
            DrawTextWithGlow(canvas, SubText, centerX, subY, subSize, ref subTextRect);
        }

        

        private void DrawTextWithGlow(SKCanvas canvas, string text, float centerX, float centerY, float fontSize, ref SKRect rect)
        {
            canvas.Save();

            // Update cached objects instead of creating new ones
            cachedFont.Size = fontSize;
            byte alpha = (byte)(currentOpacity * 255);
            textPaint.Color = faceFill.WithAlpha(alpha);
            glowPaint.Color = faceFill.WithAlpha(alpha);

            cachedFont.MeasureText(text.AsSpan(), out SKRect bounds, textPaint);
            var m = cachedFont.Metrics;
            float glyphHeight = m.Descent - m.Ascent;
            float baseline = centerY - glyphHeight / 2f - m.Ascent;

            canvas.DrawText(text, centerX, baseline, SKTextAlign.Center, cachedFont, glowPaint);
            canvas.DrawText(text, centerX, baseline, SKTextAlign.Center, cachedFont, textPaint);

            float textWidth = bounds.Width;
            rect = new SKRect(
                centerX - textWidth / 2,
                baseline + bounds.Top,
                centerX + textWidth / 2,
                baseline + bounds.Bottom
            );

            canvas.Restore();
        }

        public void CheckTouch(SKPoint point)
        {
            if (mainTextRect.Contains(point))
                MainTextClicked?.Invoke();
            else if (subTextRect.Contains(point))
                SubTextClicked?.Invoke();
        }
    }
}