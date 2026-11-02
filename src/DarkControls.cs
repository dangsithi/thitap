using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ThiTap
{
    /// <summary>Kiểu hiển thị của nút: nút thường hoặc nút chính (nền accent).</summary>
    internal enum ButtonKind
    {
        Neutral,
        Primary
    }

    /// <summary>
    /// Nền phẳng bo góc có viền 1px. BackColor của khối luôn là màu của lớp bên dưới
    /// để bốn góc bo không bị lộ hình vuông, còn màu nền thật là SurfaceColor.
    /// </summary>
    internal class SurfacePanel : Panel
    {
        public Color Fill = Theme.Card;
        public Color Outline = Theme.Border;
        public int Radius = Theme.CardRadius;

        public SurfacePanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = Theme.Body;
        }

        /// <summary>Màu nền thật của khối, để control con tô phần góc bo cho khớp.</summary>
        public virtual Color SurfaceColor
        {
            get { return Fill; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.FillBehind(e.Graphics, this);
            Theme.DrawSurface(e.Graphics, new Rectangle(0, 0, Width, Height), Radius, Fill, Outline);
            OnPaintContent(e);
        }

        protected virtual void OnPaintContent(PaintEventArgs e)
        {
        }
    }

    /// <summary>
    /// Khối nội dung bo góc (thay GroupBox): nền card sáng hơn form, viền 1px mờ,
    /// tiêu đề nhỏ phía trên để tách vùng nội dung mà không cần đường kẻ đậm.
    /// </summary>
    internal sealed class CardPanel : SurfacePanel
    {
        private string _header = string.Empty;

        public string Header
        {
            get { return _header; }
            set
            {
                _header = value == null ? string.Empty : value;
                Invalidate();
            }
        }

        protected override void OnPaintContent(PaintEventArgs e)
        {
            if (_header.Length == 0) return;
            Theme.DrawText(e.Graphics, _header, Theme.CardHeader, Theme.SubText,
                new Rectangle(Theme.S(16), Theme.S(12), Width - Theme.S(32), Theme.S(18)));
        }
    }

    /// <summary>Ô nhập lõm (nền tối hơn card) để tạo lớp phân cấp thị giác.</summary>
    internal sealed class FieldPanel : SurfacePanel
    {
        public FieldPanel()
        {
            Fill = Theme.Field;
            Radius = Theme.FieldRadius;
        }
    }

    /// <summary>Nút tự vẽ: bo góc mềm, có trạng thái rê chuột và nhấn, nút chính nền accent.</summary>
    internal sealed class FlatButton : Button
    {
        private ButtonKind _kind = ButtonKind.Neutral;
        private bool _hover;
        private bool _pressed;

        public FlatButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = Theme.Body;
            ForeColor = Theme.Text;
            BackColor = Theme.Card;
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
        }

        public ButtonKind Kind
        {
            get { return _kind; }
            set
            {
                _kind = value;
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color fill;
            Color border;
            Color text;

            if (!Enabled)
            {
                fill = Theme.DisabledFill;
                border = Theme.Border;
                text = Theme.DisabledText;
            }
            else if (_kind == ButtonKind.Primary)
            {
                fill = _pressed ? Theme.AccentPress : (_hover ? Theme.AccentHover : Theme.Accent);
                border = Color.Empty;
                text = Theme.OnAccent;
            }
            else
            {
                fill = _pressed ? Theme.ButtonPress : (_hover ? Theme.ButtonHover : Theme.ButtonRest);
                border = _hover ? Theme.BorderHover : Theme.Border;
                text = Theme.Text;
            }

            Theme.FillBehind(g, this);
            Theme.DrawSurface(g, new Rectangle(0, 0, Width, Height), Theme.S(Theme.ButtonRadius), fill, border);

            if (Focused && Enabled && _kind != ButtonKind.Primary)
            {
                using (Pen pen = new Pen(Theme.Accent))
                using (GraphicsPath path = Theme.Rounded(
                    new Rectangle(Theme.S(2), Theme.S(2), Width - Theme.S(5), Height - Theme.S(5)),
                    Theme.S(Theme.ButtonRadius - 2)))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawPath(pen, path);
                }
            }

            Theme.DrawTextCentered(g, Text, Font, text,
                new Rectangle(Theme.S(6), 0, Width - Theme.S(12), Height));
        }
    }

    /// <summary>
    /// Ô đánh dấu tự vẽ: hộp 16px bo nhẹ, khi bật thì tô accent và vẽ dấu kiểm.
    /// Kế thừa Control thay vì CheckBox vì CheckBox của Windows vẫn tự vẽ cả ô và chữ
    /// đè lên phần tự vẽ, làm chữ bị trùng.
    /// </summary>
    internal sealed class DarkCheckBox : Control
    {
        private bool _checked;
        private bool _hover;

        public DarkCheckBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable, true);
            TabStop = true;
            BackColor = Theme.Card;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            Cursor = Cursors.Hand;
        }

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Enabled || e.Button != MouseButtons.Left) return;
            Focus();
            Checked = !Checked;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Enabled && e.KeyCode == Keys.Space)
            {
                Checked = !Checked;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            Invalidate();
            base.OnTextChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.FillBehind(g, this);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int size = Theme.S(16);
            int top = (Height - size) / 2;
            Rectangle box = new Rectangle(Theme.S(1), top, size, size);

            Color fill = Checked ? Theme.Accent : Theme.Field;
            Color border = !Enabled ? Theme.Border
                : Checked ? Color.Empty
                : (_hover || Focused ? Theme.BorderHover : Theme.Border);
            Theme.DrawSurface(g, box, Theme.S(Theme.BoxRadius), fill, border);

            if (Checked)
            {
                using (Pen pen = new Pen(Enabled ? Theme.OnAccent : Theme.SubText, 2f * Theme.Scale))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLines(pen, new Point[]
                    {
                        new Point(box.X + Theme.S(4), box.Y + Theme.S(8)),
                        new Point(box.X + Theme.S(7), box.Y + Theme.S(11)),
                        new Point(box.X + Theme.S(12), box.Y + Theme.S(5))
                    });
                }
            }

            Color text = Enabled ? Theme.Text : Theme.DisabledText;
            Theme.DrawText(g, Text, Font, text,
                new Rectangle(box.Right + Theme.S(9), 0, Width - box.Right - Theme.S(9), Height));
        }
    }

    /// <summary>Nút chọn một trong nhiều tự vẽ, cùng bộ màu với ô đánh dấu.</summary>
    internal sealed class DarkRadio : Control
    {
        private bool _checked;
        private bool _hover;

        public DarkRadio()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable, true);
            TabStop = true;
            BackColor = Theme.Card;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            Cursor = Cursors.Hand;
        }

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Enabled || e.Button != MouseButtons.Left) return;
            Focus();
            // Nút chọn đã bật thì bấm lại không tắt: phải luôn có một lựa chọn.
            if (Checked) return;
            Checked = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Enabled && e.KeyCode == Keys.Space && !Checked)
            {
                Checked = true;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            Invalidate();
            base.OnTextChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.FillBehind(g, this);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int size = Theme.S(16);
            int top = (Height - size) / 2;
            Rectangle circle = new Rectangle(Theme.S(1), top, size, size);

            Color border = !Enabled ? Theme.Border
                : Checked ? Theme.Accent
                : (_hover || Focused ? Theme.BorderHover : Theme.Border);
            using (SolidBrush brush = new SolidBrush(Checked ? Theme.Accent : Theme.Field))
            {
                g.FillEllipse(brush, circle);
            }
            using (Pen pen = new Pen(border, Checked ? 2f * Theme.Scale : Math.Max(1f, Theme.Scale)))
            {
                g.DrawEllipse(pen, circle);
            }
            if (Checked)
            {
                int dot = Theme.S(6);
                Rectangle inner = new Rectangle(circle.X + Theme.S(5), circle.Y + Theme.S(5), dot, dot);
                using (SolidBrush brush = new SolidBrush(Enabled ? Theme.OnAccent : Theme.SubText))
                {
                    g.FillEllipse(brush, inner);
                }
            }

            Color text = Enabled ? Theme.Text : Theme.DisabledText;
            Theme.DrawText(g, Text, Font, text,
                new Rectangle(circle.Right + Theme.S(9), 0, Width - circle.Right - Theme.S(9), Height));
        }
    }

    /// <summary>
    /// ComboBox tối: mặt hiển thị và danh sách đều tự vẽ nên không lộ nền sáng của Windows.
    /// ComboBox khoá chiều cao theo font, nên chỉ chỉnh được màu chứ không chỉnh được kích thước.
    /// </summary>
    internal sealed class DarkComboBox : ComboBox
    {
        private bool _hover;

        public DarkComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            BackColor = Theme.Field;
            ForeColor = Theme.Text;
            Font = Theme.Body;
            ItemHeight = Theme.S(22);
            Cursor = Cursors.Hand;
            DrawItem += OnDrawItem;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Theme.DarkScrollBar(this);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Windows tự vẽ mặt ComboBox rồi mới tới đây; vẽ đè lên bằng màu tối của ThiTap.
        /// Vẽ kín cả hình chữ nhật (kể cả góc) để không còn góc vuông sáng lọt ra ngoài.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            const int WmPaint = 0x000F;
            if (m.Msg != WmPaint || !IsHandleCreated) return;

            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                PaintFace(g);
            }
        }

        private void PaintFace(Graphics g)
        {
            Rectangle bounds = ClientRectangle;

            using (SolidBrush brush = new SolidBrush(Ui.SurfaceBehind(Parent)))
            {
                g.FillRectangle(brush, bounds);
            }

            Color fill = !Enabled ? Theme.DisabledFill : (_hover || DroppedDown ? Theme.FieldHover : Theme.Field);
            Color border = !Enabled ? Theme.Border : (Focused ? Theme.Accent : Theme.Border);
            Theme.DrawSurface(g, bounds, Theme.FieldRadius, fill, border);

            const int arrow = 20;
            Color text = Enabled ? Theme.Text : Theme.DisabledText;
            string caption = SelectedIndex >= 0 ? GetItemText(Items[SelectedIndex]) : string.Empty;
            Theme.DrawText(g, caption, Font, text,
                new Rectangle(Theme.S(10), 0, bounds.Width - Theme.S(10) - arrow, bounds.Height));

            // Mũi tên xuống nhỏ, vẽ bằng hai nét để không cần ảnh.
            int cx = bounds.Right - arrow / 2 - Theme.S(5);
            int cy = bounds.Height / 2 - Theme.S(1);
            using (Pen pen = new Pen(Enabled ? Theme.SubText : Theme.DisabledText, 1.6f * Theme.Scale))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLines(pen, new Point[]
                {
                    new Point(cx - Theme.S(4), cy - Theme.S(2)),
                    new Point(cx, cy + Theme.S(2)),
                    new Point(cx + Theme.S(4), cy - Theme.S(2))
                });
            }
        }

        private void OnDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool selected = (e.State & DrawItemState.Selected) != 0;
            using (SolidBrush brush = new SolidBrush(selected ? Theme.FieldHover : Theme.Field))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            if (selected)
            {
                using (SolidBrush brush = new SolidBrush(Theme.Selection))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }

            Theme.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, Theme.Text,
                new Rectangle(e.Bounds.X + Theme.S(9), e.Bounds.Y, e.Bounds.Width - Theme.S(12), e.Bounds.Height));
        }
    }

    /// <summary>
    /// Danh sách tối: nền ô nhập, dòng đang chọn phủ accent mờ.
    /// </summary>
    internal static class DarkList
    {
        /// <summary>Chiều cao mỗi dòng, tính theo DPI hiện tại.</summary>
        public static int RowHeight
        {
            get { return Theme.S(24); }
        }

        /// <summary>Chỉ đặt thuộc tính tối cho danh sách, không gắn bộ vẽ dòng.</summary>
        public static void StyleBase(ListBox list)
        {
            list.BorderStyle = BorderStyle.None;
            list.BackColor = Theme.Field;
            list.ForeColor = Theme.Text;
            list.Font = Theme.Body;
            list.IntegralHeight = false;
            list.DrawMode = DrawMode.OwnerDrawFixed;
            list.ItemHeight = RowHeight;
            list.HandleCreated += delegate { Theme.DarkScrollBar(list); };
            if (list.IsHandleCreated) Theme.DarkScrollBar(list);
        }

        /// <summary>Danh sách thường: mỗi dòng chỉ có chữ.</summary>
        public static void Style(ListBox list)
        {
            StyleBase(list);
            list.DrawItem += delegate(object sender, DrawItemEventArgs e)
            {
                if (e.Index < 0) return;
                PaintRow(e.Graphics, e.Bounds, IsSelected(e), list.GetItemText(list.Items[e.Index]),
                    list.Font, e.Bounds.X + Theme.S(12));
            };
        }

        /// <summary>Tô nền dòng: nền ô nhập, dòng đang chọn phủ thêm accent mờ.</summary>
        public static void FillRow(Graphics g, Rectangle bounds, bool selected)
        {
            using (SolidBrush brush = new SolidBrush(Theme.Field))
            {
                g.FillRectangle(brush, bounds);
            }
            if (selected)
            {
                using (SolidBrush brush = new SolidBrush(Theme.Selection))
                {
                    g.FillRectangle(brush, bounds);
                }
            }
        }

        /// <summary>Vẽ chữ của dòng bắt đầu từ textLeft.</summary>
        public static void PaintText(Graphics g, Rectangle bounds, string text, Font font, int textLeft)
        {
            Theme.DrawText(g, text, font, Theme.Text,
                new Rectangle(textLeft, bounds.Y, bounds.Right - textLeft - Theme.S(4), bounds.Height));
        }

        /// <summary>Tô nền rồi vẽ chữ của một dòng không có ô đánh dấu.</summary>
        public static void PaintRow(Graphics g, Rectangle bounds, bool selected, string text, Font font,
            int textLeft)
        {
            FillRow(g, bounds, selected);
            PaintText(g, bounds, text, font, textLeft);
        }

        /// <summary>Vẽ ô đánh dấu đầu dòng, trả về vị trí bắt đầu của chữ.</summary>
        public static int PaintCheck(Graphics g, Rectangle bounds, bool isChecked)
        {
            int size = Theme.S(15);
            int top = bounds.Y + (bounds.Height - size) / 2;
            Rectangle box = new Rectangle(bounds.X + Theme.S(8), top, size, size);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            Theme.DrawSurface(g, box, Theme.S(Theme.BoxRadius),
                isChecked ? Theme.Accent : Theme.FieldHover,
                isChecked ? Color.Empty : Theme.BorderHover);

            if (isChecked)
            {
                using (Pen pen = new Pen(Theme.OnAccent, 2f * Theme.Scale))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLines(pen, new Point[]
                    {
                        new Point(box.X + Theme.S(4), box.Y + Theme.S(8)),
                        new Point(box.X + Theme.S(7), box.Y + Theme.S(10)),
                        new Point(box.X + Theme.S(11), box.Y + Theme.S(5))
                    });
                }
            }

            return box.Right + Theme.S(10);
        }

        public static bool IsSelected(DrawItemEventArgs e)
        {
            return (e.State & DrawItemState.Selected) != 0;
        }
    }

    /// <summary>
    /// Danh sách có ô đánh dấu, tự quản lý trạng thái thay vì CheckedListBox của Windows:
    /// CheckedListBox luôn vẽ ô đánh dấu kiểu hệ thống nên không làm tối được.
    /// </summary>
    internal sealed class DarkCheckList : ListBox
    {
        private readonly List<bool> _states = new List<bool>();

        public DarkCheckList()
        {
            DarkList.StyleBase(this);
            SelectionMode = SelectionMode.One;
            DrawItem += delegate(object sender, DrawItemEventArgs e)
            {
                if (e.Index < 0) return;
                DarkList.FillRow(e.Graphics, e.Bounds, DarkList.IsSelected(e));
                int textLeft = DarkList.PaintCheck(e.Graphics, e.Bounds, GetChecked(e.Index));
                DarkList.PaintText(e.Graphics, e.Bounds, GetItemText(Items[e.Index]), Font, textLeft);
            };
        }

        public void AddItem(string text, bool isChecked)
        {
            _states.Add(isChecked);
            Items.Add(text);
        }

        public void ClearItems()
        {
            _states.Clear();
            Items.Clear();
        }

        public bool GetChecked(int index)
        {
            return index >= 0 && index < _states.Count && _states[index];
        }

        public void SetChecked(int index, bool value)
        {
            if (index < 0 || index >= _states.Count || _states[index] == value) return;
            _states[index] = value;
            Invalidate();
        }

        public int CheckedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _states.Count; i++)
                {
                    if (_states[i]) count++;
                }
                return count;
            }
        }

        public string[] CheckedNames()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < _states.Count; i++)
            {
                if (_states[i] && i < Items.Count) names.Add((string)Items[i]);
            }
            return names.ToArray();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int index = IndexFromPoint(e.Location);
            if (index < 0) return;
            SetChecked(index, !GetChecked(index));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && SelectedIndex >= 0)
            {
                SetChecked(SelectedIndex, !GetChecked(SelectedIndex));
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
    }
}
