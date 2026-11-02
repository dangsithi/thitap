using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ThiTap
{
    /// <summary>
    /// Thiết kế icon dùng chung cho khay hệ thống và icon nhúng trong file exe.
    /// Đang gõ tiếng Việt: cờ Việt Nam (nền đỏ, sao vàng).
    /// Đang dùng tiếng Anh (không gõ tiếng Việt): cờ Anh (nền trắng, thánh giá đỏ).
    /// </summary>
    internal static class IconDesign
    {
        private static readonly Color VietnamRed = Color.FromArgb(0xDA, 0x25, 0x1D);
        private static readonly Color VietnamYellow = Color.FromArgb(0xFF, 0xDD, 0x00);
        private static readonly Color VietnamEdge = Color.FromArgb(60, 0, 0, 0);

        private static readonly Color EnglandWhite = Color.White;
        private static readonly Color EnglandRed = Color.FromArgb(0xCE, 0x11, 0x24);
        private static readonly Color EnglandEdge = Color.FromArgb(0xB0, 0xB0, 0xB0);

        /// <summary>Bề rộng thánh giá cờ Anh, tính theo tỉ lệ cạnh icon.</summary>
        private const float CrossWidthRatio = 0.24f;

        /// <summary>Bán kính ngoài của sao vàng, tính theo tỉ lệ cạnh icon.</summary>
        private const float StarRadiusRatio = 0.33f;

        /// <summary>Bán kính trong của sao vàng, tính theo tỉ lệ bán kính ngoài.</summary>
        private const float StarInnerRatio = 0.382f;

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        /// <summary>Tạo <see cref="Icon"/> cho khay hệ thống và thanh tiêu đề cửa sổ.</summary>
        public static Icon CreateIcon(int size, bool vietnamese)
        {
            using (Bitmap bitmap = Render(size, vietnamese))
            {
                IntPtr handle = bitmap.GetHicon();
                try
                {
                    using (Icon temporary = Icon.FromHandle(handle))
                    {
                        return (Icon)temporary.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
        }

        /// <summary>Vẽ icon kích thước <paramref name="size"/> pixel, nền trong suốt quanh phần bo góc.</summary>
        public static Bitmap Render(int size, bool vietnamese)
        {
            Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.Clear(Color.Transparent);

                float inset = Math.Max(0.5f, size * 0.02f);
                float side = size - inset * 2f;
                float radius = Math.Max(1.5f, size * 0.16f);
                float center = size / 2f;

                using (GraphicsPath flag = RoundedSquare(inset, inset, side, side, radius))
                {
                    if (vietnamese)
                    {
                        DrawVietnamFlag(graphics, flag, side, center);
                    }
                    else
                    {
                        DrawEnglandFlag(graphics, flag, inset, side, center);
                    }
                }
            }
            return bitmap;
        }

        /// <summary>Cờ Việt Nam: nền đỏ tươi, sao vàng 5 cánh ở giữa.</summary>
        private static void DrawVietnamFlag(Graphics graphics, GraphicsPath flag, float side, float center)
        {
            using (SolidBrush red = new SolidBrush(VietnamRed))
            {
                graphics.FillPath(red, flag);
            }

            using (SolidBrush yellow = new SolidBrush(VietnamYellow))
            {
                graphics.FillPolygon(yellow, StarPoints(center, center, side * StarRadiusRatio));
            }

            using (Pen edge = new Pen(VietnamEdge, EdgeWidth(side)))
            {
                graphics.DrawPath(edge, flag);
            }
        }

        /// <summary>Cờ Anh (St George): nền trắng, thánh giá đỏ chạy giữa.</summary>
        private static void DrawEnglandFlag(Graphics graphics, GraphicsPath flag, float inset, float side, float center)
        {
            using (SolidBrush white = new SolidBrush(EnglandWhite))
            {
                graphics.FillPath(white, flag);
            }

            // Cắt theo viền bo góc để hai thanh thánh giá không tràn ra ngoài.
            GraphicsState state = graphics.Save();
            graphics.SetClip(flag);
            float arm = side * CrossWidthRatio;
            using (SolidBrush red = new SolidBrush(EnglandRed))
            {
                graphics.FillRectangle(red, inset, center - arm / 2f, side, arm);
                graphics.FillRectangle(red, center - arm / 2f, inset, arm, side);
            }
            graphics.Restore(state);

            using (Pen edge = new Pen(EnglandEdge, EdgeWidth(side)))
            {
                graphics.DrawPath(edge, flag);
            }
        }

        /// <summary>
        /// 10 đỉnh của sao 5 cánh (một đỉnh hướng lên), xen kẽ bán kính ngoài và bán kính trong
        /// để cạnh sao không bị lõm quá sâu ở kích thước 16 pixel.
        /// </summary>
        private static PointF[] StarPoints(float centerX, float centerY, float radius)
        {
            PointF[] points = new PointF[10];
            for (int i = 0; i < points.Length; i++)
            {
                double angle = -Math.PI / 2.0 + i * Math.PI / 5.0;
                float r = (i % 2 == 0) ? radius : radius * StarInnerRatio;
                points[i] = new PointF(
                    centerX + (float)(Math.Cos(angle) * r),
                    centerY + (float)(Math.Sin(angle) * r));
            }
            return points;
        }

        /// <summary>Viền mảnh, đủ thấy ở 16 pixel và không bị dày ở 256 pixel.</summary>
        private static float EdgeWidth(float side)
        {
            float width = side * 0.02f;
            if (width < 1f) return 1f;
            if (width > 3f) return 3f;
            return width;
        }

        private static GraphicsPath RoundedSquare(float x, float y, float width, float height, float radius)
        {
            float diameter = radius * 2f;
            if (diameter > width) diameter = width;
            if (diameter > height) diameter = height;

            GraphicsPath path = new GraphicsPath();
            path.AddArc(x, y, diameter, diameter, 180f, 90f);
            path.AddArc(x + width - diameter, y, diameter, diameter, 270f, 90f);
            path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(x, y + height - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }
    }
}
