using System;
using System.Drawing;
using System.Windows.Forms;

namespace ThiTap
{
    /// <summary>
    /// Tạo các control theo giao diện tối để mọi hộp thoại dùng chung một bộ màu,
    /// cùng bán kính bo góc và cùng khoảng cách lề.
    /// </summary>
    internal static class Ui
    {
        public const int Pad = 16;

        /// <summary>
        /// Đang nạp cài đặt vào giao diện: các handler đổi giá trị tạm thời không ghi ngược
        /// lại vào Settings (nếu không sẽ ghi đè khi mở hộp thoại).
        /// </summary>
        public static bool Loading;

        /// <summary>
        /// Màu của lớp nền ngay dưới một control cha: nền card, nền ô nhập hay nền form.
        /// Dùng để các control tự vẽ tô đúng bốn góc bo của chính nó.
        /// </summary>
        public static Color SurfaceBehind(Control parent)
        {
            SurfacePanel surface = parent as SurfacePanel;
            if (surface != null) return surface.SurfaceColor;
            return parent == null ? Theme.Background : parent.BackColor;
        }

        public static CardPanel Card(Control parent, string header, int x, int y, int width, int height)
        {
            CardPanel card = new CardPanel();
            card.Header = header;
            card.Location = new Point(Theme.S(x), Theme.S(y));
            card.Size = new Size(Theme.S(width), Theme.S(height));
            parent.Controls.Add(card);
            return card;
        }

        public static FieldPanel Field(Control parent, int x, int y, int width, int height)
        {
            FieldPanel field = new FieldPanel();
            field.BackColor = SurfaceBehind(parent);
            field.Location = new Point(Theme.S(x), Theme.S(y));
            field.Size = new Size(Theme.S(width), Theme.S(height));
            parent.Controls.Add(field);
            return field;
        }

        public static Label Title(Control parent, string text, int x, int y, int width)
        {
            return MakeLabel(parent, text, x, y, width, 30, Theme.Title, Theme.Text);
        }

        public static Label Label(Control parent, string text, int x, int y, int width)
        {
            return MakeLabel(parent, text, x, y, width, 20, Theme.Body, Theme.Text);
        }

        public static Label Note(Control parent, string text, int x, int y, int width)
        {
            return MakeLabel(parent, text, x, y, width, 18, Theme.Small, Theme.SubText);
        }

        /// <summary>Đoạn chú thích nhiều dòng: chiều cao tự tính theo bề rộng cho trước.</summary>
        public static Label Paragraph(Control parent, string text, int x, int y, int width)
        {
            int height = MeasureHeight(text, Theme.Small, Theme.S(width));
            return PlaceLabel(parent, text, x, y, width, height, Theme.Small, Theme.SubText);
        }

        public static int MeasureHeight(string text, Font font, int width)
        {
            Size measured = TextRenderer.MeasureText(text, font, new Size(width, 0), TextFormatFlags.WordBreak);
            return measured.Height + 4;
        }

        private static Label MakeLabel(Control parent, string text, int x, int y, int width, int height,
            Font font, Color color)
        {
            return PlaceLabel(parent, text, x, y, width, Theme.S(height), font, color);
        }

        /// <summary>Tạo nhãn với kích thước đã là điểm ảnh (bề cao đo được, không nhân tỉ lệ).</summary>
        private static Label PlaceLabel(Control parent, string text, int x, int y, int width, int height,
            Font font, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(Theme.S(x), Theme.S(y));
            label.Size = new Size(Theme.S(width), height);
            label.AutoSize = false;
            label.Font = font;
            label.ForeColor = color;
            label.BackColor = SurfaceBehind(parent);
            label.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(label);
            return label;
        }

        public static FlatButton Button(Control parent, string text, int x, int y, int width, int height,
            EventHandler onClick)
        {
            return MakeButton(parent, text, x, y, width, height, false, onClick);
        }

        public static FlatButton PrimaryButton(Control parent, string text, int x, int y, int width, int height,
            EventHandler onClick)
        {
            return MakeButton(parent, text, x, y, width, height, true, onClick);
        }

        private static FlatButton MakeButton(Control parent, string text, int x, int y, int width, int height,
            bool primary, EventHandler onClick)
        {
            FlatButton button = new FlatButton();
            button.Text = text;
            button.Kind = primary ? ButtonKind.Primary : ButtonKind.Neutral;
            button.Location = new Point(Theme.S(x), Theme.S(y));
            button.Size = new Size(Theme.S(width), Theme.S(height));
            button.BackColor = SurfaceBehind(parent);
            if (onClick != null) button.Click += onClick;
            parent.Controls.Add(button);
            return button;
        }

        public static DarkCheckBox Check(Control parent, string text, int x, int y, int width, bool value,
            Action<bool> apply)
        {
            return Check(parent, text, x, y, width, value, true, apply);
        }

        public static DarkCheckBox Check(Control parent, string text, int x, int y, int width, bool value,
            bool supported, Action<bool> apply)
        {
            DarkCheckBox box = new DarkCheckBox();
            box.Text = text;
            box.Location = new Point(Theme.S(x), Theme.S(y));
            box.Size = new Size(Theme.S(width), Theme.S(22));
            box.BackColor = SurfaceBehind(parent);
            box.Enabled = supported;
            box.Checked = value;
            if (apply != null)
            {
                box.CheckedChanged += delegate
                {
                    if (Loading || !box.Enabled) return;
                    apply(box.Checked);
                };
            }
            parent.Controls.Add(box);
            return box;
        }

        public static DarkRadio Radio(Control parent, string text, int x, int y, int width, bool value,
            Action onSelect)
        {
            DarkRadio radio = new DarkRadio();
            radio.Text = text;
            radio.Location = new Point(Theme.S(x), Theme.S(y));
            radio.Size = new Size(Theme.S(width), Theme.S(22));
            radio.BackColor = SurfaceBehind(parent);
            radio.Checked = value;
            if (onSelect != null)
            {
                radio.CheckedChanged += delegate
                {
                    if (!Loading && radio.Checked) onSelect();
                };
            }
            parent.Controls.Add(radio);
            return radio;
        }

        public static DarkComboBox Combo(Control parent, string[] items, int x, int y, int width, int selected,
            Action<int> onChange, bool enabled)
        {
            DarkComboBox combo = new DarkComboBox();
            combo.Location = new Point(Theme.S(x), Theme.S(y));
            combo.Width = Theme.S(width);
            combo.Enabled = enabled;
            if (items != null) combo.Items.AddRange(items);
            if (combo.Items.Count > 0) combo.SelectedIndex = Math.Max(0, Math.Min(selected, combo.Items.Count - 1));
            if (onChange != null)
            {
                combo.SelectedIndexChanged += delegate
                {
                    if (!Loading) onChange(combo.SelectedIndex);
                };
            }
            parent.Controls.Add(combo);
            return combo;
        }

        /// <summary>Ô nhập một dòng nằm trong khung lõm bo góc; trả về TextBox để lấy nội dung.</summary>
        public static TextBox TextField(Control parent, int x, int y, int width, int height)
        {
            FieldPanel field = Field(parent, x, y, width, height);

            TextBox box = new TextBox();
            box.BorderStyle = BorderStyle.None;
            box.BackColor = Theme.Field;
            box.ForeColor = Theme.Text;
            box.Font = Theme.Body;
            box.Width = Theme.S(width - 20);
            box.Location = new Point(Theme.S(10), Math.Max(1, Theme.S((height - 16) / 2)));
            field.Controls.Add(box);

            field.HandleCreated += delegate { Theme.DarkScrollBar(field); };
            return box;
        }

        public static ListBox List(Control parent, int x, int y, int width, int height)
        {
            ListBox list = new ListBox();
            DarkList.Style(list);
            return FinishList(parent, list, x, y, width, height);
        }

        public static DarkCheckList CheckList(Control parent, int x, int y, int width, int height)
        {
            DarkCheckList list = new DarkCheckList();
            return (DarkCheckList)FinishList(parent, list, x, y, width, height);
        }

        private static ListBox FinishList(Control parent, ListBox list, int x, int y, int width, int height)
        {
            FieldPanel field = Field(parent, x, y, width, height);
            list.Size = new Size(Theme.S(width - 6), Theme.S(height - 6));
            list.Location = new Point(Theme.S(3), Theme.S(3));
            field.Controls.Add(list);

            ClipInner(list);
            list.SizeChanged += delegate { ClipInner(list); };
            return list;
        }

        private static void ClipInner(Control list)
        {
            Theme.ClipRounded(list, Theme.S(Theme.FieldRadius - 2));
        }
    }
}
