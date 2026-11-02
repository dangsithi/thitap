using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ThiTap
{
    /// <summary>
    /// Hộp thoại thông báo tối thay cho MessageBox: khung bo góc, tiêu đề đậm, nội dung
    /// màu phụ, một nút chính màu accent. Enter hoặc Esc đều đóng được.
    /// </summary>
    internal sealed class DarkBox : Form
    {
        private const int Radius = 12;
        private const int BodyWidth = 432;

        private readonly bool _warning;

        private DarkBox(string title, string text, bool warning)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;
            Theme.Apply(this);

            _warning = warning;

            int bodyHeight = Ui.MeasureHeight(text, Theme.Body, BodyWidth);
            int bodyTop = 72;
            int buttonTop = bodyTop + bodyHeight + 20;
            ClientSize = new Size(480, buttonTop + 32 + 24);

            Label heading = Ui.Title(this, title, 66, 26, 390);
            heading.Height = 26;

            Label body = Ui.Label(this, text, 24, bodyTop, BodyWidth);
            body.Height = bodyHeight;
            body.ForeColor = Theme.SubText;
            body.TextAlign = ContentAlignment.TopLeft;

            Ui.PrimaryButton(this, "Đóng", ClientSize.Width - 24 - 104, buttonTop, 104, 32,
                delegate { Close(); });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.DrawSurface(e.Graphics, new Rectangle(0, 0, Width, Height), Radius,
                Theme.Background, Theme.Border);

            // Biểu tượng tròn nhỏ: chữ "i" cho thông tin, dấu "!" cho cảnh báo.
            Rectangle circle = new Rectangle(24, 26, 30, 30);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new SolidBrush(Theme.Accent))
            {
                e.Graphics.FillEllipse(brush, circle);
            }
            Theme.DrawTextCentered(e.Graphics, _warning ? "!" : "i", Theme.BodyBold, Theme.OnAccent, circle);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Theme.ClipRounded(this, Radius);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
            {
                Close();
                return;
            }
            base.OnKeyDown(e);
        }

        public static void Info(IWin32Window owner, string title, string text)
        {
            Show(owner, title, text, false);
        }

        public static void Warn(IWin32Window owner, string title, string text)
        {
            Show(owner, title, text, true);
        }

        private static void Show(IWin32Window owner, string title, string text, bool warning)
        {
            using (DarkBox box = new DarkBox(title, text, warning))
            {
                box.ShowDialog(owner);
            }
        }
    }

    /// <summary>Bảng màu cho menu khay hệ thống để không còn menu trắng của Windows.</summary>
    internal sealed class DarkMenuColors : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground { get { return Theme.Card; } }
        public override Color ImageMarginGradientBegin { get { return Theme.Card; } }
        public override Color ImageMarginGradientMiddle { get { return Theme.Card; } }
        public override Color ImageMarginGradientEnd { get { return Theme.Card; } }
        public override Color MenuBorder { get { return Theme.Border; } }
        public override Color MenuItemBorder { get { return Theme.Border; } }
        public override Color MenuItemSelected { get { return Theme.ButtonHover; } }
        public override Color MenuItemSelectedGradientBegin { get { return Theme.ButtonHover; } }
        public override Color MenuItemSelectedGradientEnd { get { return Theme.ButtonHover; } }
        public override Color MenuItemPressedGradientBegin { get { return Theme.ButtonPress; } }
        public override Color MenuItemPressedGradientEnd { get { return Theme.ButtonPress; } }
        public override Color SeparatorDark { get { return Theme.Border; } }
        public override Color SeparatorLight { get { return Theme.Border; } }
        public override Color CheckBackground { get { return Theme.Card; } }
        public override Color CheckSelectedBackground { get { return Theme.ButtonHover; } }
        public override Color CheckPressedBackground { get { return Theme.ButtonHover; } }
    }

    /// <summary>Vẽ menu khay hệ thống theo bảng màu tối, dấu chọn màu accent.</summary>
    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkMenuColors())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Theme.Card))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            Rectangle bounds = new Rectangle(0, 0, e.ToolStrip.Width, e.ToolStrip.Height);
            using (Pen pen = new Pen(Theme.Border))
            {
                e.Graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
            Color fill = e.Item.Selected ? Theme.ButtonHover : Theme.Card;
            Theme.DrawSurface(e.Graphics, bounds, 6, fill, Color.Empty);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? Theme.Text : Theme.DisabledText;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Rectangle bounds = e.Item.Bounds;
            int y = bounds.Height / 2;
            using (Pen pen = new Pen(Theme.Border))
            {
                e.Graphics.DrawLine(pen, bounds.Left + 10, y, bounds.Right - 10, y);
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle bounds = e.ImageRectangle;
            int size = Math.Min(bounds.Width, bounds.Height) - 4;
            if (size <= 0) return;

            Rectangle box = new Rectangle(bounds.X + (bounds.Width - size) / 2,
                bounds.Y + (bounds.Height - size) / 2, size, size);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(Theme.Accent, 2f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                e.Graphics.DrawLines(pen, new Point[]
                {
                    new Point(box.X + 1, box.Y + size / 2),
                    new Point(box.X + size / 3, box.Bottom - 2),
                    new Point(box.Right - 1, box.Y + 1)
                });
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Theme.SubText;
            base.OnRenderArrow(e);
        }
    }
}
