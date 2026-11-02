using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ThiTap
{
    /// <summary>
    /// Bảng màu tối dùng chung: nền #202020, card #2B2B2B, chữ #F3F3F3 / #A0A0A0,
    /// nhấn #4CC2FF, viền #3D3D3D. Phân cấp thị giác dựa vào các lớp nền
    /// (nền form → card → ô nhập lõm) và góc bo mềm, viền chỉ 1px mờ.
    /// </summary>
    internal static class Theme
    {
        public static readonly Color Background = Rgb(0x20, 0x20, 0x20);
        public static readonly Color Card = Rgb(0x2B, 0x2B, 0x2B);
        public static readonly Color Field = Rgb(0x1F, 0x1F, 0x1F);
        public static readonly Color FieldHover = Rgb(0x26, 0x26, 0x26);
        public static readonly Color ButtonRest = Rgb(0x32, 0x32, 0x32);
        public static readonly Color ButtonHover = Rgb(0x3C, 0x3C, 0x3C);
        public static readonly Color ButtonPress = Rgb(0x45, 0x45, 0x45);
        public static readonly Color Text = Rgb(0xF3, 0xF3, 0xF3);
        public static readonly Color SubText = Rgb(0xA0, 0xA0, 0xA0);
        public static readonly Color Accent = Rgb(0x4C, 0xC2, 0xFF);
        public static readonly Color AccentHover = Rgb(0x60, 0xCD, 0xFF);
        public static readonly Color AccentPress = Rgb(0x3A, 0xA9, 0xE0);
        public static readonly Color OnAccent = Rgb(0x0F, 0x22, 0x2C);
        public static readonly Color Border = Rgb(0x3D, 0x3D, 0x3D);
        public static readonly Color BorderHover = Rgb(0x55, 0x55, 0x55);
        public static readonly Color DisabledFill = Rgb(0x26, 0x26, 0x26);
        public static readonly Color DisabledText = Rgb(0x6E, 0x6E, 0x6E);
        public static readonly Color Selection = Color.FromArgb(56, 0x4C, 0xC2, 0xFF);

        // Bán kính bo góc: card mềm hơn nút, ô nhập nhỏ nhất.
        public const int CardRadius = 10;
        public const int ButtonRadius = 7;
        public const int FieldRadius = 7;
        public const int BoxRadius = 4;

        public static readonly Font Body = new Font("Segoe UI", 9f);
        public static readonly Font BodyBold = new Font("Segoe UI Semibold", 9f);
        public static readonly Font Small = new Font("Segoe UI", 8.25f);
        public static readonly Font CardHeader = new Font("Segoe UI Semibold", 9.5f);
        public static readonly Font Title = new Font("Segoe UI Semibold", 15f);
        public static readonly Font DialogTitle = new Font("Segoe UI Semibold", 12f);

        private static readonly StringFormat Near = CreateFormat(StringAlignment.Near, StringAlignment.Center);
        private static readonly StringFormat Center = CreateFormat(StringAlignment.Center, StringAlignment.Center);
        private static readonly StringFormat WrapNear = CreateWrapFormat();

        private static Color Rgb(int r, int g, int b)
        {
            return Color.FromArgb(r, g, b);
        }

        private static StringFormat CreateFormat(StringAlignment alignment, StringAlignment line)
        {
            StringFormat format = new StringFormat(StringFormat.GenericTypographic);
            format.Alignment = alignment;
            format.LineAlignment = line;
            format.FormatFlags |= StringFormatFlags.NoWrap;
            format.Trimming = StringTrimming.EllipsisCharacter;
            return format;
        }

        private static StringFormat CreateWrapFormat()
        {
            StringFormat format = new StringFormat(StringFormat.GenericTypographic);
            format.Alignment = StringAlignment.Near;
            format.LineAlignment = StringAlignment.Near;
            format.Trimming = StringTrimming.EllipsisWord;
            return format;
        }

        /// <summary>Áp màu tối cho một form: nền, chữ, font và thanh tiêu đề.</summary>
        public static void Apply(Form form)
        {
            form.BackColor = Background;
            form.ForeColor = Text;
            form.Font = Body;
            if (form.IsHandleCreated)
            {
                DarkTitleBar(form.Handle);
            }
            else
            {
                form.HandleCreated += delegate { DarkTitleBar(form.Handle); };
            }
        }

        /// <summary>
        /// Tô kín nền control bằng BackColor trước khi vẽ khối bo góc. Control tự vẽ chỉ tô
        /// phần trong đường bo, nên nếu không tô trước thì bốn góc còn trơ lại đệm nền cũ
        /// của Windows và hiện thành màu đen.
        /// </summary>
        public static void FillBehind(Graphics g, Control control)
        {
            using (SolidBrush brush = new SolidBrush(control.BackColor))
            {
                g.FillRectangle(brush, control.ClientRectangle);
            }
        }

        private static float _scale = -1f;

        /// <summary>
        /// Tỉ lệ phóng to theo DPI của màn hình: 1.0 là 96 DPI (100%), 1.5 là 150%.
        /// Bố cục vẫn viết theo đơn vị 96 DPI rồi nhân với tỉ lệ này, còn chữ dùng đơn vị
        /// point nên Windows tự phóng to — nhờ vậy khung và chữ không bị lệch nhau.
        /// </summary>
        public static float Scale
        {
            get
            {
                if (_scale < 0f) _scale = ReadScale();
                return _scale;
            }
        }

        private static float ReadScale()
        {
            try
            {
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    float scale = g.DpiX / 96f;
                    return scale > 0f ? scale : 1f;
                }
            }
            catch (Exception)
            {
                return 1f;
            }
        }

        /// <summary>Số đo thiết kế (96 DPI) sang điểm ảnh của màn hình hiện tại.</summary>
        public static int S(int value)
        {
            return (int)Math.Round(value * Scale, MidpointRounding.AwayFromZero);
        }

        /// <summary>Vẽ một khối bo góc có viền 1px; viền truyền Color.Empty nếu không cần.</summary>
        public static void DrawSurface(Graphics g, Rectangle bounds, int radius, Color fill, Color border)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (GraphicsPath path = Rounded(rect, radius))
            {
                if (fill != Color.Empty)
                {
                    using (SolidBrush brush = new SolidBrush(fill)) g.FillPath(brush, path);
                }
                if (border != Color.Empty)
                {
                    using (Pen pen = new Pen(border)) g.DrawPath(pen, path);
                }
            }
        }

        /// <summary>Đường bo góc dùng cả để vẽ và để cắt vùng (Region) của control con.</summary>
        public static GraphicsPath Rounded(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            if (radius <= 0 || bounds.Width <= d || bounds.Height <= d)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Cắt control con theo góc bo để nền chữ nhật không tràn ra ngoài khung.</summary>
        public static void ClipRounded(Control control, int radius)
        {
            using (GraphicsPath path = Rounded(control.ClientRectangle, radius))
            {
                control.Region = new Region(path);
            }
        }

        public static void DrawText(Graphics g, string text, Font font, Color color, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(text) || bounds.Width <= 0) return;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.DrawString(text, font, brush, bounds, Near);
            }
        }

        public static void DrawTextCentered(Graphics g, string text, Font font, Color color, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(text) || bounds.Width <= 0) return;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.DrawString(text, font, brush, bounds, Center);
            }
        }

        public static void DrawTextWrapped(Graphics g, string text, Font font, Color color, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(text) || bounds.Width <= 0) return;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.DrawString(text, font, brush, bounds, WrapNear);
            }
        }

        /// <summary>
        /// Thanh tiêu đề tối của Windows: bật chế độ tối của DWM rồi tô màu tiêu đề
        /// và viền theo bảng màu. Máy cũ không hỗ trợ thì bỏ qua, không báo lỗi.
        /// </summary>
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string appName, string idList);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr module, IntPtr ordinal);

        private delegate int PreferredAppModeDelegate(int mode);

        /// <summary>
        /// Bật chế độ tối cho những phần do Windows vẽ (thanh cuộn của danh sách, khung
        /// danh sách sổ xuống) qua hàm nội bộ của uxtheme. Bản Windows không có hàm này
        /// thì giữ nguyên mặc định, không báo lỗi.
        /// </summary>
        public static void EnableSystemDarkMode()
        {
            try
            {
                IntPtr uxtheme = LoadLibrary("uxtheme.dll");
                if (uxtheme == IntPtr.Zero) return;

                IntPtr setMode = GetProcAddress(uxtheme, (IntPtr)135);   // SetPreferredAppMode
                if (setMode != IntPtr.Zero)
                {
                    PreferredAppModeDelegate mode = (PreferredAppModeDelegate)
                        Marshal.GetDelegateForFunctionPointer(setMode, typeof(PreferredAppModeDelegate));
                    mode(2);   // 2 = ForceDark
                }

                IntPtr flush = GetProcAddress(uxtheme, (IntPtr)136);     // FlushMenuThemes
                if (flush != IntPtr.Zero)
                {
                    Action flushThemes = (Action)Marshal.GetDelegateForFunctionPointer(flush, typeof(Action));
                    flushThemes();
                }
            }
            catch (Exception)
            {
                // Hàm nội bộ đổi theo bản Windows: không dùng được thì thôi.
            }
        }

        private static void DarkTitleBar(IntPtr handle)
        {
            try
            {
                int on = 1;
                // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE (Windows 10 2004+), 19 là mã của bản cũ hơn.
                if (DwmSetWindowAttribute(handle, 20, ref on, 4) != 0)
                {
                    DwmSetWindowAttribute(handle, 19, ref on, 4);
                }

                // 35/36/34 = màu tiêu đề, màu chữ tiêu đề, màu viền (Windows 11).
                int caption = ToColorRef(Background);
                DwmSetWindowAttribute(handle, 35, ref caption, 4);
                int captionText = ToColorRef(Text);
                DwmSetWindowAttribute(handle, 36, ref captionText, 4);
                int frame = ToColorRef(Border);
                DwmSetWindowAttribute(handle, 34, ref frame, 4);
            }
            catch (DllNotFoundException)
            {
                // Windows không có dwmapi (không xảy ra với Windows Vista trở lên).
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        private static int ToColorRef(Color color)
        {
            return color.R | (color.G << 8) | (color.B << 16);
        }

        /// <summary>Thanh cuộn của control có sẵn (danh sách, ô nhập) cũng tối theo.</summary>
        public static void DarkScrollBar(Control control)
        {
            if (!control.IsHandleCreated) return;
            try
            {
                SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }
    }
}
