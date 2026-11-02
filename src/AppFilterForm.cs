using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ThiTap
{
    /// <summary>
    /// Chọn những ứng dụng không dùng tiếng Việt (ví dụ game, trình biên dịch).
    /// Danh sách lưu trong apps.txt, mỗi dòng một tên tiến trình.
    /// </summary>
    internal sealed class AppFilterForm : Form
    {
        private CheckedListBox _list;
        private TextBox _manual;
        private Label _status;

        public AppFilterForm()
        {
            Text = "Tắt tiếng Việt theo ứng dụng - ThiTap";
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(420, 430);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.None;
            ShowInTaskbar = false;

            BuildUi();
            LoadRunning();
        }

        private void BuildUi()
        {
            Label hint = new Label();
            hint.Text = "Đánh dấu những ứng dụng cần tắt tiếng Việt:";
            hint.Location = new Point(12, 10);
            hint.Size = new Size(396, 18);
            Controls.Add(hint);

            _list = new CheckedListBox();
            _list.Location = new Point(12, 32);
            _list.Size = new Size(396, 280);
            _list.IntegralHeight = false;
            _list.CheckOnClick = true;
            Controls.Add(_list);

            Label manualLabel = new Label();
            manualLabel.Text = "Thêm tên khác:";
            manualLabel.Location = new Point(12, 322);
            manualLabel.Size = new Size(92, 18);
            Controls.Add(manualLabel);

            _manual = new TextBox();
            _manual.Location = new Point(108, 318);
            _manual.Size = new Size(200, 23);
            Controls.Add(_manual);

            Button add = new Button();
            add.Text = "Thêm";
            add.Location = new Point(316, 318);
            add.Size = new Size(92, 24);
            add.Click += delegate { AddManual(); };
            Controls.Add(add);

            Button save = new Button();
            save.Text = "Lưu";
            save.Location = new Point(12, 352);
            save.Size = new Size(100, 28);
            save.Click += delegate { SaveList(); };
            Controls.Add(save);

            Button close = new Button();
            close.Text = "Đóng";
            close.Location = new Point(308, 352);
            close.Size = new Size(100, 28);
            close.Click += delegate
            {
                SaveList();
                Close();
            };
            Controls.Add(close);

            _status = new Label();
            _status.Location = new Point(12, 386);
            _status.Size = new Size(396, 32);
            _status.ForeColor = SystemColors.GrayText;
            _status.Text = "Chưa thay đổi. Danh sách lưu tại: " + AppFilter.FilePath;
            Controls.Add(_status);
        }

        /// <summary>Các tiến trình đang chạy cộng với tên đã lưu, bỏ trùng.</summary>
        private void LoadRunning()
        {
            List<string> names = new List<string>();
            string[] saved = AppFilter.All;
            for (int i = 0; i < saved.Length; i++) names.Add(saved[i]);

            try
            {
                Process[] running = Process.GetProcesses();
                for (int i = 0; i < running.Length; i++)
                {
                    try
                    {
                        string name = running[i].ProcessName;
                        if (name.Length == 0) continue;
                        if (string.Equals(name, "ThiTap", StringComparison.OrdinalIgnoreCase)) continue;
                        if (!names.Contains(name)) names.Add(name);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    finally
                    {
                        running[i].Dispose();
                    }
                }
            }
            catch (Exception)
            {
                // Không lấy được danh sách tiến trình vẫn dùng được danh sách đã lưu.
            }

            names.Sort(StringComparer.OrdinalIgnoreCase);

            _list.BeginUpdate();
            _list.Items.Clear();
            for (int i = 0; i < names.Count; i++)
            {
                bool disable = AppFilter.IsDisabledFor(names[i]);
                _list.Items.Add(names[i], disable);
            }
            _list.EndUpdate();
        }

        private void AddManual()
        {
            string name = _manual.Text.Trim();
            if (name.Length == 0) return;

            name = name.ToLowerInvariant();
            if (name.EndsWith(".exe", StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - 4);
            }

            for (int i = 0; i < _list.Items.Count; i++)
            {
                if (string.Equals((string)_list.Items[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    _list.SetItemChecked(i, true);
                    _status.Text = "Đã đánh dấu \"" + name + "\".";
                    _manual.Text = string.Empty;
                    return;
                }
            }

            _list.Items.Add(name, true);
            _manual.Text = string.Empty;
            _status.Text = "Đã thêm \"" + name + "\".";
        }

        private void SaveList()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < _list.Items.Count; i++)
            {
                if (_list.GetItemChecked(i)) names.Add((string)_list.Items[i]);
            }

            AppFilter.Set(names.ToArray());
            _status.Text = "Đã lưu " + names.Count + " ứng dụng tắt tiếng Việt.";
        }
    }
}
