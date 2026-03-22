using Avi.face.Animation;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Avi.face
{
    public class Face : SKCanvasView
    {
        public bool StartupMode { get; set; } = true;

        public const float DesignWidth = 800f;
        public const float DesignHeight = 450f;

        private readonly FaceTextController textController;

        // Eyes
        public float EyeOpacity { get; set; } = 0.0f;
        public float LeftEyeScaleY { get; set; } = 0.05f;
        public float RightEyeScaleY { get; set; } = 0.05f;
        public float LeftEyeScared { get; set; } = 0f;
        public float RightEyeScared { get; set; } = 0f;
        public float LeftEyeAddtionHappy { get; set; } = 150f;
        public float RightEyeAddtionHappy { get; set; } = 150f;
        public float EyeHeight => 180f;
        public float LeftEyeBaseY => 135f;
        public float RightEyeBaseY => 135f;
        public float LeftEyeCenterY => LeftEyeBaseY + EyeHeight * 0.5f;

        // Brows
        public float LeftBrowOpacity { get; set; } = 0f;
        public float RightBrowOpacity { get; set; } = 0f;
        public float LeftBrowRotation { get; set; } = 0f;
        public float RightBrowRotation { get; set; } = 0f;
        public float LeftBrowTranslateX { get; set; } = 0f;
        public float RightBrowTranslateX { get; set; } = 0f;
        public float LeftBrowTranslateY { get; set; } = 0f;
        public float RightBrowTranslateY { get; set; } = 0f;
        public float LeftBrowBaseY => 103.803f;
        public float RightBrowBaseY => 103.803f;

        // Extras
        public float WarningSymbolOpacity { get; set; } = 1f;
        public float BrowClipInset { get; set; } = -5f;
        public float PupilRadius { get; set; } = 30f;

        // Colors
        private readonly SKColor faceFill = SKColor.Parse("#FEFFED");
        private readonly SKColor glowColor = SKColor.Parse("#d1bb45");
        public SKColor LeftEyeFillColor { get; set; } = SKColor.Parse("#FEFFED");
        public SKColor RightEyeFillColor { get; set; } = SKColor.Parse("#FEFFED");

        // Cached paints
        private readonly SKPaint paintEyeStroke;
        private readonly SKPaint paintEyeFill;
        private readonly SKPaint paintShadowStroke;
        private readonly SKPaint paintShadowFill;
        private readonly SKPaint paintBrow;
        private readonly SKPaint paintWave;
        private readonly SKPaint paintWarning;
        private readonly SKImageFilter shadowFilter;

        public event Action? MainTextClicked;
        public event Action? SubTextClicked;

        public string MainText
        {
            get => textController.MainText;
            set { textController.MainText = value; InvalidateSurface(); }
        }
        public float TextOpacity
        {
            get => textController.targetOpacity;
            set { textController.targetOpacity = value; InvalidateSurface(); }
        }

        public string SubText
        {
            get => textController.SubText;
            set { textController.SubText = value; InvalidateSurface(); }
        }

        public Face()
        {
            IgnorePixelScaling = true;
            EnableTouchEvents = true;
            BackgroundColor = Colors.Transparent;

            shadowFilter = SKImageFilter.CreateDropShadowOnly(0, 0, 10, 10, glowColor);

            paintEyeStroke = new SKPaint { Style = SKPaintStyle.Stroke, Color = faceFill, StrokeWidth = 20f, IsAntialias = true };
            paintEyeFill = new SKPaint { Style = SKPaintStyle.Fill, Color = faceFill, IsAntialias = true };
            
            paintShadowStroke = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 20f, Color = glowColor, IsAntialias = true, ImageFilter = shadowFilter };
            paintShadowFill = new SKPaint { Style = SKPaintStyle.Fill, Color = glowColor, IsAntialias = true, ImageFilter = shadowFilter };
            
            paintBrow = new SKPaint { Style = SKPaintStyle.Stroke, Color = faceFill, StrokeWidth = 18f, StrokeCap = SKStrokeCap.Round, IsAntialias = true, ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 10, 10, glowColor) };
            paintWave = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 10f, StrokeCap = SKStrokeCap.Round, Color = faceFill, IsAntialias = true, ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 10, 10, glowColor) };
            paintWarning = new SKPaint { Style = SKPaintStyle.Fill, Color = faceFill, IsAntialias = true, ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 10, 10, glowColor) };

            textController = new FaceTextController(faceFill, glowColor);
            

            textController.MainTextClicked += () => MainTextClicked?.Invoke();
            textController.SubTextClicked += () => SubTextClicked?.Invoke();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            var info = e.Info;

            float scaleX = info.Width / DesignWidth;
            float scaleY = info.Height / DesignHeight;
            float scale = Math.Min(scaleX, scaleY);

            float offsetX = (info.Width - DesignWidth * scale) / 2f;
            float offsetY = (info.Height - DesignHeight * scale) / 2f;

            canvas.Save();
            canvas.Translate(offsetX, offsetY);
            canvas.Scale(scale, scale);

            canvas.SaveLayer(new SKPaint { Color = SKColors.White.WithAlpha((byte)(Opacity * 255)) });

            if (!StartupMode)
            {
                DrawEyes(canvas);
                DrawBrows(canvas);
                DrawEyesScared(canvas);
                DrawWarningSymbol(canvas);
            }

            float centerY = DesignHeight / 2;
            float fontScale = scale * 0.7f;
            
            textController.Draw(canvas, DesignWidth / 2, centerY - 20, 80 * fontScale, centerY + 40, 50 * fontScale);

            canvas.Restore();
            canvas.Restore();
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.Released)
            {
                var info = CanvasSize;
                float scaleX = info.Width / DesignWidth;
                float scaleY = info.Height / DesignHeight;
                float scale = Math.Min(scaleX, scaleY);

                float offsetX = (info.Width - DesignWidth * scale) / 2f;
                float offsetY = (info.Height - DesignHeight * scale) / 2f;

                float x = (e.Location.X - offsetX) / scale;
                float y = (e.Location.Y - offsetY) / scale;

                textController.CheckTouch(new SKPoint(x, y));
            }
            base.OnTouch(e);
            e.Handled = true;
        }

        public void Update(float dt) => textController.Update(dt);

        public void FadeOutBreathing(float duration = 0.3f)
        {
            textController.FreezeBreathing = true;
            textController.targetOpacity = 1f;
            textController.fadeSpeed = 1f / duration;
        }

        private void DrawEyes(SKCanvas canvas)
        {
            float eyeWidth = 75f;
            float eyeHeight = 180f;
            float stroke = 20f;
            float browBaseY = 103.803f;
            float browLength = 150f;

            var leftBrowBaseX = 176f + eyeWidth / 2 - 75;
            var rightBrowBaseX = 549f + eyeWidth / 2 - 75;

            var leftBrowP = FaceMath.ComputeBrowEndpoints(leftBrowBaseX, browBaseY, LeftBrowRotation, LeftBrowTranslateX, LeftBrowTranslateY, browLength);
            var rightBrowP = FaceMath.ComputeBrowEndpoints(rightBrowBaseX, browBaseY, RightBrowRotation, RightBrowTranslateX, RightBrowTranslateY, browLength);

            DrawEye(canvas, 176f, 135f, eyeWidth, eyeHeight, stroke, LeftEyeScaleY, true, leftBrowP.Item1, leftBrowP.Item2);
            DrawEye(canvas, 549f, 135f, eyeWidth, eyeHeight, stroke, RightEyeScaleY, false, rightBrowP.Item1, rightBrowP.Item2);
        }

        private void DrawEye(SKCanvas canvas, float x, float y, float w, float h, float stroke, float scaleY, bool isLeft, SKPoint browP1Global, SKPoint browP2Global)
        {
            var eyeColor = isLeft ? LeftEyeFillColor : RightEyeFillColor;
            float cx = x + w / 2f;
            float cy = y + h / 2f;
            float pY = isLeft ? LeftEyeAddtionHappy : RightEyeAddtionHappy;

            canvas.Save();
            canvas.Translate(cx, cy);

            float a = w / 2f;
            float b = Math.Max((h / 2f) * scaleY, 0.1f);

            // IMPORTANT: Using statements to prevent memory leaks during rapid redraws
            using var fullEllipse = new SKPath();
            fullEllipse.AddOval(new SKRect(-a, -b, a, b));

            var browP1 = new SKPoint(browP1Global.X - cx, browP1Global.Y - cy);
            var browP2 = new SKPoint(browP2Global.X - cx, browP2Global.Y - cy);
            
            using var browMask = FaceMath.BuildBrowMaskPath(browP1, browP2, BrowClipInset);
            using var visibleEye = fullEllipse.Op(browMask, SKPathOp.Difference);

            if (visibleEye == null || visibleEye.IsEmpty)
            {
                canvas.Restore();
                return;
            }

            float pupilYf = pY;
            float pupilRf = PupilRadius;

            using var pupil = new SKPath();
            pupil.AddOval(new SKRect(-PupilRadius, pupilYf - pupilRf, PupilRadius, pupilYf + pupilRf));

            using var eyeFill = visibleEye.Op(pupil, SKPathOp.Difference);
            using var browClippedEllipse = fullEllipse.Op(browMask, SKPathOp.Difference);
            using var outlineArea = new SKPath();
            
            using (var p = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = stroke })
            {
                p.GetFillPath(browClippedEllipse, outlineArea);
            }

            using var shadowArea = outlineArea.Op(pupil, SKPathOp.Difference);

            // Draw shadow
            canvas.DrawPath(shadowArea, paintShadowFill);

            // Draw eye and outline
            canvas.Save();
            canvas.ClipPath(pupil, SKClipOperation.Difference, true);

            //byte alpha = (byte)(255 * EyeOpacity);
            paintEyeFill.Color = eyeColor;
            canvas.DrawPath(eyeFill, paintEyeFill);
            
            canvas.ClipPath(browMask, SKClipOperation.Difference, true);
            paintEyeStroke.Color = faceFill;
            paintEyeStroke.StrokeWidth = stroke;
            canvas.DrawPath(fullEllipse, paintEyeStroke);

            canvas.Restore();
            canvas.Restore();
        }

        private void DrawBrows(SKCanvas canvas)
        {
            float eyeWidth = 75f;
            var leftBrowBaseX = 176f + eyeWidth / 2 - 75;
            var rightBrowBaseX = 549f + eyeWidth / 2 - 75;

            DrawBrow(canvas, leftBrowBaseX, 103.803f, LeftBrowOpacity, LeftBrowRotation, LeftBrowTranslateX, LeftBrowTranslateY);
            DrawBrow(canvas, rightBrowBaseX, 103.803f, RightBrowOpacity, RightBrowRotation, RightBrowTranslateX, RightBrowTranslateY);
        }

        private void DrawBrow(SKCanvas canvas, float x, float y, float opacity, float rotation, float tx, float ty)
        {
            if (opacity <= 0.001f) return;

            float browLength = 150f;
            canvas.Save();
            canvas.Translate(x + tx, y + ty);
            canvas.Translate(browLength / 2, 0);
            canvas.RotateDegrees(rotation);
            canvas.Translate(-browLength / 2, 0);

            paintBrow.Color = faceFill.WithAlpha((byte)(opacity * 255));
            canvas.DrawLine(0, 0, browLength, 0, paintBrow);
            canvas.Restore();
        }

        private void DrawEyesScared(SKCanvas canvas)
        {
            float leftEyeCenterX = 176f + 75f / 2f + 70;
            float leftEyeCenterY = 135f + 180f / 2f + 110;
            float rightEyeCenterX = 549f + 75f / 2f - 70;
            float rightEyeCenterY = 135f + 180f / 2f + 110;

            DrawEyeScared(canvas, leftEyeCenterX, leftEyeCenterY, 140f, LeftEyeScared, false);
            DrawEyeScared(canvas, rightEyeCenterX, rightEyeCenterY, 140f, RightEyeScared, true);
        }

        private void DrawEyeScared(SKCanvas canvas, float eyeCenterX, float eyeCenterY, float rotation, float opacity, bool isRight)
        {
            if (opacity <= 0.001f) return;

            canvas.Save();
            canvas.Translate(eyeCenterX, eyeCenterY);
            canvas.Scale(isRight ? -1f : 1f, 1f);
            canvas.RotateDegrees(rotation);

            using var path = new SKPath();
            path.MoveTo(70, 30);
            path.QuadTo(117, 71, 70, 160);

            paintWave.Color = faceFill.WithAlpha((byte)(opacity * 255));
            canvas.DrawPath(path, paintWave);
            canvas.Restore();
        }

        private void DrawWarningSymbol(SKCanvas canvas)
        {
            if (WarningSymbolOpacity <= 0.001f) return;

            float cx = 549f + 75f * 0.5f;
            float cy = 135f + 180f * 0.5f;

            canvas.Save();
            canvas.Translate(cx, cy);

            using var paint = new SKPaint
            {
                Color = faceFill.WithAlpha((byte)(WarningSymbolOpacity * 255)),
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 10, 10, glowColor)
            };

            var typeface = SKTypeface.FromFamilyName("Segoe UI Symbol");
            using var font = new SKFont(typeface, 200f);
            font.MeasureText("⚠", out SKRect bounds);

            float textCenterX = -(bounds.Left + bounds.Right) / 2f;
            float textCenterY = -(bounds.Top + bounds.Bottom) / 2f;

            canvas.DrawText("⚠", textCenterX, textCenterY, SKTextAlign.Left, font, paint);
            canvas.Restore();
        }
    }
}