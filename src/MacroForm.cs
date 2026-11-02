using System;
using System.Drawing;
using System.Windows.Forms;

namespace ThiTap
{
    /// <summary>
    /// Soạn bảng gõ tắt: gõ khoá rồi bấm dấu cách để thay bằng nội dung tương ứng.
    /// Giao diện tối cùng bộ màu với hộp thoại cấu hình; mọi thay đổi ghi ngay vào macros.txt.
    /// </summary>
    internal sealed class MacroForm : Form
    {
        private ListBox _list;
        private TextBox _trigger;
        private TextBox _replacement;
        private Label _status;

        public MacroForm()
        {
            Text = "Bảng gõ tắt - ThiTap";
            ClientSize = new Size(480, 384);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.None;
            ShowInTaskbar = false;

            Theme.Apply(this);

            BuildUi();
            Reload();
        }

        private void BuildUi()
        {
            const int left = Ui.Pad;
            const int width = 448;

            Ui.Note(this, "Gõ từ khoá rồi bấm dấu cách để thay bằng nội dung bên phải.", left, 14, width);

            _list = Ui.List(this, left, 40, width, 200);
            _list.SelectedIndexChanged += delegate { LoadSelected(); };

            Ui.Label(this, "Từ khoá", left, 252, 56);
            _trigger = Ui.TextField(this, left + 60, 248, 130, 28);

            Ui.Label(this, "Thay bằng", left + 200, 252, 70);
            _replacement = Ui.TextField(this, left + 274, 248, 174, 28);

            Ui.Button(this, "Thêm / Sửa", left, 286, 116, 30, delegate { SaveEntry(); });
            Ui.Button(this, "Xoá", left + 124, 286, 88, 30, delegate { RemoveEntry(); });
            Ui.PrimaryButton(this, "Đóng", left + width - 96, 286, 96, 30, delegate { Close(); });

            _status = Ui.Note(this, string.Empty, left, 326, width);
            Ui.Note(this, "Lưu tại: " + MacroStore.FilePath, left, 346, width);
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
