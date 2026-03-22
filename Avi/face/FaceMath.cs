using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avi.face
{
    public static class FaceMath
    {
        public static (SKPoint, SKPoint) ComputeBrowEndpoints(float x, float y, float rotation, float tx, float ty, float browLength)
        {
            var p1 = new SKPoint(0f, 0f);
            var p2 = new SKPoint(browLength, 0f);

            var rp1 = RotateAround(p1, browLength / 2f, 0f, rotation);
            var rp2 = RotateAround(p2, browLength / 2f, 0f, rotation);

            rp1 = new SKPoint(rp1.X + x + tx, rp1.Y + y + ty);
            rp2 = new SKPoint(rp2.X + x + tx, rp2.Y + y + ty);

            return (rp1, rp2);
        }

        private static SKPoint RotateAround(SKPoint p, float pivotX, float pivotY, float angleDegrees)
        {
            double angle = angleDegrees * Math.PI / 180.0;
            double s = Math.Sin(angle);
            double c = Math.Cos(angle);

            double px = p.X - pivotX;
            double py = p.Y - pivotY;

            double rx = px * c - py * s;
            double ry = px * s + py * c;

            return new SKPoint((float)(rx + pivotX), (float)(ry + pivotY));
        }

        public static SKPath BuildBrowMaskPath(SKPoint p1, SKPoint p2, float insetOffset)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len < 0.1f) len = 0.1f;

            float nx = dy / len;
            float ny = -dx / len;

            SKPoint c1 = new SKPoint(p1.X + nx * insetOffset, p1.Y + ny * insetOffset);
            SKPoint c2 = new SKPoint(p2.X + nx * insetOffset, p2.Y + ny * insetOffset);

            const float BIG = 5000f;
            var path = new SKPath();
            path.MoveTo(c1);
            path.LineTo(c2);
            path.LineTo(c2.X + nx * BIG, c2.Y + ny * BIG);
            path.LineTo(c1.X + nx * BIG, c1.Y + ny * BIG);
            path.Close();
            return path;
        }
    }
}
