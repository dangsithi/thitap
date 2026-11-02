using System;
using System.Drawing;
using System.Windows.Forms;

namespace ThiTap
{
    /// <summary>
    /// Soạn bảng gõ tắt: gõ khoá rồi bấm dấu cách để thay bằng nội dung tương ứng.
    /// Mọi thay đổi ghi ngay vào macros.txt.
    /// </summary>
    internal sealed class MacroForm : Form
    {
        private ListBox _list;
        private TextBox _trigger;
        private TextBox _replacement;
        private Button _save;
        private Button _remove;
        private Label _status;

        public MacroForm()
        {
            Text = "Bảng gõ tắt - ThiTap";
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(440, 350);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.None;
            ShowInTaskbar = false;

            BuildUi();
            Reload();
        }

        private void BuildUi()
        {
            Label hint = new Label();
            hint.Text = "Gõ từ khoá rồi bấm dấu cách để thay bằng nội dung bên phải.";
            hint.Location = new Point(12, 10);
            hint.Size = new Size(416, 18);
            Controls.Add(hint);

            _list = new ListBox();
            _list.Location = new Point(12, 32);
            _list.Size = new Size(416, 180);
            _list.IntegralHeight = false;
            _list.SelectedIndexChanged += delegate { LoadSelected(); };
            Controls.Add(_list);

            Label triggerLabel = new Label();
            triggerLabel.Text = "Từ khoá:";
            triggerLabel.Location = new Point(12, 226);
            triggerLabel.Size = new Size(60, 18);
            Controls.Add(triggerLabel);

            _trigger = new TextBox();
            _trigger.Location = new Point(76, 222);
            _trigger.Size = new Size(110, 23);
            Controls.Add(_trigger);

            Label replacementLabel = new Label();
            replacementLabel.Text = "Thay bằng:";
            replacementLabel.Location = new Point(196, 226);
            replacementLabel.Size = new Size(72, 18);
            Controls.Add(replacementLabel);

            _replacement = new TextBox();
            _replacement.Location = new Point(270, 222);
            _replacement.Size = new Size(158, 23);
            Controls.Add(_replacement);

            _save = new Button();
            _save.Text = "Thêm / Sửa";
            _save.Location = new Point(12, 254);
            _save.Size = new Size(110, 28);
            _save.Click += delegate { SaveEntry(); };
            Controls.Add(_save);

            _remove = new Button();
            _remove.Text = "Xoá";
            _remove.Location = new Point(128, 254);
            _remove.Size = new Size(80, 28);
            _remove.Click += delegate { RemoveEntry(); };
            Controls.Add(_remove);

            Button close = new Button();
            close.Text = "Đóng";
            close.Location = new Point(348, 254);
            close.Size = new Size(80, 28);
            close.Click += delegate { Close(); };
            Controls.Add(close);

            _status = new Label();
            _status.Location = new Point(12, 292);
            _status.Size = new Size(416, 18);
            _status.ForeColor = SystemColors.GrayText;
            Controls.Add(_status);

            Label path = new Label();
            path.Text = "Lưu tại: " + MacroStore.FilePath;
            path.Location = new Point(12, 312);
            path.Size = new Size(416, 18);
            path.ForeColor = SystemColors.GrayText;
            Controls.Add(path);
        }

        private void Reload()
        {
            int selected = _list.SelectedIndex;
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (MacroStore.Entry entry in MacroStore.All)
            {
                _list.Items.Add(entry);
            }
            _list.EndUpdate();

            if (selected >= 0 && selected < _list.Items.Count) _list.SelectedIndex = selected;
        }

        private void LoadSelected()
        {
            if (_list.SelectedIndex < 0)
            {
                _trigger.Text = string.Empty;
                _replacement.Text = string.Empty;
                return;
            }

            MacroStore.Entry entry = (MacroStore.Entry)_list.SelectedItem;
            _trigger.Text = entry.Trigger;
            _replacement.Text = entry.Replacement;
        }

        private void SaveEntry()
        {
            string trigger = _trigger.Text.Trim();
            string replacement = _replacement.Text.Trim();

            if (trigger.Length == 0 || replacement.Length == 0)
            {
                _status.Text = "Cần nhập cả từ khoá và nội dung thay thế.";
                return;
            }

            if (trigger.IndexOf(' ') >= 0 || trigger.IndexOf('=') >= 0)
            {
                _status.Text = "Từ khoá không được chứa dấu cách hoặc dấu =.";
                return;
            }

            // Trùng khoá: cập nhật chính mục đó thay vì tạo thêm dòng mới.
            int existing = -1;
            for (int i = 0; i < _list.Items.Count; i++)
            {
                MacroStore.Entry item = (MacroStore.Entry)_list.Items[i];
                if (!string.Equals(item.Trigger, trigger, StringComparison.OrdinalIgnoreCase)) continue;
                existing = i;
                break;
            }

            if (existing >= 0)
            {
                MacroStore.Update(existing, trigger, replacement);
            }
            else if (_list.SelectedIndex >= 0)
            {
                MacroStore.Update(_list.SelectedIndex, trigger, replacement);
            }
            else
            {
                MacroStore.Add(trigger, replacement);
            }

            _status.Text = "Đã lưu \"" + trigger + "\".";
            Reload();
            ClearInputs();
        }

        private void RemoveEntry()
        {
            int index = _list.SelectedIndex;
            if (index < 0)
            {
                _status.Text = "Chọn một dòng cần xoá.";
                return;
            }

            MacroStore.Entry entry = (MacroStore.Entry)_list.Items[index];
            MacroStore.Remove(index);
            _status.Text = "Đã xoá \"" + entry.Trigger + "\".";
            Reload();
            ClearInputs();
        }

        private void ClearInputs()
        {
            _list.ClearSelected();
            _trigger.Text = string.Empty;
            _replacement.Text = string.Empty;
        }
    }
}
