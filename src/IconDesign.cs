using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace ThiTap
{
    /// <summary>
    /// Thiết kế icon dùng chung cho icon khay hệ thống và icon nhúng trong file exe.
    /// Ô vuông bo góc: đỏ tươi + chữ V vàng (đang gõ tiếng Việt), xanh dương + chữ E trắng (English).
    /// </summary>
    internal static class IconDesign
    {
        private static readonly Color RedBackground = Color.FromArgb(233, 30, 34);
        private static readonly Color GoldLetter = Color.FromArgb(255, 205, 40);
        private static readonly Color BlueBackground = Color.FromArgb(28, 108, 220);
        private static readonly Color WhiteLetter = Color.White;

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
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                graphics.Clear(Color.Transparent);

                float inset = Math.Max(0.5f, size * 0.02f);
                float side = size - inset * 2f;
                float radius = Math.Max(1.5f, size * 0.22f);

                using (GraphicsPath path = RoundedSquare(inset, inset, side, side, radius))
                using (SolidBrush brush = new SolidBrush(vietnamese ? RedBackground : BlueBackground))
                {
                    graphics.FillPath(brush, path);
                }

                using (Font font = new Font("Segoe UI", size * 0.6f, FontStyle.Bold, GraphicsUnit.Pixel))
                using (SolidBrush letterBrush = new SolidBrush(vietnamese ? GoldLetter : WhiteLetter))
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    graphics.DrawString(vietnamese ? "V" : "E", font, letterBrush,
                        new RectangleF(0f, 0f, size, size), format);
                }
            }
            return bitmap;
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
