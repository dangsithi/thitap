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
        private DarkCheckList _list;
        private TextBox _manual;
        private Label _status;

        public AppFilterForm()
        {
            Text = "Tắt tiếng Việt theo ứng dụng - ThiTap";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.None;
            ShowInTaskbar = false;

            Theme.Apply(this);

            BuildUi();
            LoadRunning();
        }

        private void BuildUi()
        {
            const int left = Ui.Pad;
            const int width = 428;

            Ui.Note(this, "Đánh dấu những ứng dụng cần tắt tiếng Việt:", left, 14, width);

            _list = Ui.CheckList(this, left, 40, width, 300);

            Ui.Label(this, "Thêm tên khác", left, 352, 90);
            _manual = Ui.TextField(this, left + 94, 348, 190, 28);
            Ui.Button(this, "Thêm", left + 294, 346, 88, 30, delegate { AddManual(); });

            Ui.Button(this, "Lưu", left, 388, 100, 30, delegate { SaveList(); });
            Ui.PrimaryButton(this, "Đóng", left + width - 100, 388, 100, 30, delegate
            {
                SaveList();
                Close();
            });

            _status = Ui.Paragraph(this,
                "Chưa thay đổi. Danh sách lưu tại: " + AppFilter.FilePath, left, 426, width);

            ClientSize = new Size(Theme.S(left * 2 + width), _status.Bottom + Theme.S(Ui.Pad));
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
            _list.ClearItems();
            for (int i = 0; i < names.Count; i++)
            {
                bool disable = AppFilter.IsDisabledFor(names[i]);
                _list.AddItem(names[i], disable);
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
                    _list.SetChecked(i, true);
                    _status.Text = "Đã đánh dấu \"" + name + "\".";
                    _manual.Text = string.Empty;
                    return;
                }
            }

            _list.AddItem(name, true);
            _manual.Text = string.Empty;
            _status.Text = "Đã thêm \"" + name + "\".";
        }

        private void SaveList()
        {
            string[] names = _list.CheckedNames();
            AppFilter.Set(names);
            _status.Text = "Đã lưu " + names.Length + " ứng dụng tắt tiếng Việt.";
        }
    }
}
