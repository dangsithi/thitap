using System;
using System.Drawing;
using System.Windows.Forms;

namespace ThiTap
{
    /// <summary>
    /// Hộp thoại cấu hình bố cục theo UniKey. Mỗi thay đổi được lưu và áp dụng ngay,
    /// nên nút "Đóng" chỉ để ẩn hộp thoại. Các mục chưa hỗ trợ bị làm mờ.
    /// </summary>
    internal sealed class ConfigForm : Form
    {
        private readonly Action _settingsChanged;
        private readonly Action _exitApp;
        private bool _loading;

        private ComboBox _typingMethod;
        private RadioButton _hotkeyCtrlShift;
        private RadioButton _hotkeyAltZ;
        private CheckBox _freeTyping;
        private CheckBox _oldStyleTone;
        private CheckBox _spellCheck;
        private CheckBox _showNotification;
        private CheckBox _beepOnSwitch;
        private CheckBox _showDialogOnStart;
        private CheckBox _autoStart;
        private CheckBox _clipboard;
        private CheckBox _autoRestore;
        private CheckBox _macroEnabled;
        private CheckBox _macroWhenOff;
        private CheckBox _appFilter;
        private MacroForm _macroForm;
        private AppFilterForm _appForm;

        public ConfigForm(Action settingsChanged, Action exitApp)
        {
            _settingsChanged = settingsChanged;
            _exitApp = exitApp;

            Text = "ThiTap 1.0";
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(500, 464);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.None;

            Icon icon = IconDesign.CreateIcon(32, true);
            Icon = icon;

            BuildUi();
            LoadValues();
        }

        private void BuildUi()
        {
            GroupBox controlGroup = AddGroup("Điều khiển", 8, 8, 484, 130);

            AddLabel(controlGroup, "Bảng mã:", 12, 24, 72);
            AddCombo(controlGroup, "Unicode", 88, 21, 150);
            AddLabel(controlGroup, "Kiểu gõ:", 12, 56, 72);
            _typingMethod = AddChoice(controlGroup, new string[] { "Telex", "VNI" }, 88, 53, 150,
                Settings.VniMode ? 1 : 0, delegate(int index)
                {
                    Settings.InputMethod = index == 1 ? Settings.InputVni : Settings.InputTelex;
                });
            AddButton(controlGroup, "...", 244, 53, 28, 23, null, false);

            AddLabel(controlGroup, "Phím chuyển:", 12, 90, 88);
            _hotkeyCtrlShift = AddRadio(controlGroup, "CTRL + SHIFT", 100, 88, 112, Settings.HotkeyCtrlShift);
            _hotkeyAltZ = AddRadio(controlGroup, "ALT + Z", 218, 88, 90, Settings.HotkeyAltZ);

            AddButton(controlGroup, "✓ Đóng", 326, 20, 146, 28, delegate { Hide(); }, true);
            AddButton(controlGroup, "× Kết thúc", 326, 55, 146, 28, delegate
            {
                if (_exitApp != null) _exitApp();
            }, true);
            AddButton(controlGroup, "— Thu nhỏ", 326, 90, 146, 28, delegate { Hide(); }, true);

            GroupBox optionGroup = AddGroup("Tùy chọn khác", 8, 146, 484, 130);

            _freeTyping = AddCheck(optionGroup, "Cho phép gõ tự do", 12, 24, 232, true,
                delegate(bool value) { Settings.FreeTyping = value; });
            _oldStyleTone = AddCheck(optionGroup, "Đặt dấu oà, uý (thay vì oá, úy)", 12, 48, 232, true,
                delegate(bool value) { Settings.OldStyleTone = value; });
            _clipboard = AddCheck(optionGroup, "Luôn sử dụng clipboard cho Unicode", 12, 72, 232, true,
                delegate(bool value) { Settings.ClipboardMode = value; });

            _spellCheck = AddCheck(optionGroup, "Bật kiểm tra chính tả", 252, 24, 232, true,
                delegate(bool value) { Settings.SpellCheck = value; });
            _autoRestore = AddCheck(optionGroup, "Tự động khôi phục phím tiếng Anh", 252, 48, 232, true,
                delegate(bool value) { Settings.AutoRestore = value; });
            _showNotification = AddCheck(optionGroup, "Hiện thông báo phản hồi", 252, 72, 232, true,
                delegate(bool value) { Settings.ShowNotification = value; });
            _beepOnSwitch = AddCheck(optionGroup, "Bật chuông khi chuyển ngôn ngữ", 252, 96, 232, true,
                delegate(bool value) { Settings.BeepOnSwitch = value; });

            GroupBox macroGroup = AddGroup("Tùy chọn gõ tắt", 8, 284, 244, 116);
            _macroEnabled = AddCheck(macroGroup, "Cho phép gõ tắt", 12, 22, 220, true,
                delegate(bool value) { Settings.MacroEnabled = value; });
            _macroWhenOff = AddCheck(macroGroup, "Gõ tắt cả khi tắt tiếng Việt", 12, 46, 220, true,
                delegate(bool value) { Settings.MacroWhenOff = value; });
            AddButton(macroGroup, "Bảng gõ tắt...", 12, 74, 150, 26, delegate { ShowMacros(); }, true);

            GroupBox systemGroup = AddGroup("Hệ thống", 256, 284, 236, 116);
            _showDialogOnStart = AddCheck(systemGroup, "Bật hội thoại này khi khởi động", 12, 22, 214, true,
                delegate(bool value) { Settings.ShowDialogOnStart = value; });
            _autoStart = AddCheck(systemGroup, "Khởi động cùng Windows", 12, 46, 214, true, SetAutoStart);
            _appFilter = AddCheck(systemGroup, "Bật/tắt theo ứng dụng", 12, 70, 148, true,
                delegate(bool value) { Settings.AppFilter = value; });
            AddButton(systemGroup, "Chọn...", 166, 68, 62, 23, delegate { ShowApps(); }, true);

            // Giao diện đã là tiếng Việt nên mục này luôn bật và không cần thay đổi.
            CheckBox vietnameseUi = AddCheck(systemGroup, "Vietnamese interface", 12, 94, 214, false, null);
            vietnameseUi.Checked = true;

            Label note = new Label();
            note.Text = "Bảng mã: chỉ Unicode. Các mục bị làm mờ chưa có trong bản này.";
            note.Location = new Point(8, 404);
            note.Size = new Size(484, 16);
            note.ForeColor = SystemColors.GrayText;
            Controls.Add(note);

            AddButton(this, "Hướng dẫn", 8, 424, 110, 28, delegate { ShowHelp(); }, true);
            AddButton(this, "Thông tin", 126, 424, 100, 28, delegate { ShowAbout(); }, true);
            AddButton(this, "Mặc định", 232, 424, 100, 28, delegate { ResetToDefaults(); }, true);
        }

        private GroupBox AddGroup(string text, int x, int y, int width, int height)
        {
            GroupBox group = new GroupBox();
            group.Text = text;
            group.Location = new Point(x, y);
            group.Size = new Size(width, height);
            Controls.Add(group);
            return group;
        }

        private void AddLabel(Control parent, string text, int x, int y, int width)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x, y);
            label.Size = new Size(width, 18);
            parent.Controls.Add(label);
        }

        private void AddCombo(Control parent, string text, int x, int y, int width)
        {
            ComboBox combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Items.Add(text);
            combo.SelectedIndex = 0;
            combo.Location = new Point(x, y);
            combo.Size = new Size(width, 23);

            // Bảng mã Unicode là cố định trong bản này; kiểu gõ đã chọn được bằng AddChoice.
            combo.Enabled = false;
            parent.Controls.Add(combo);
        }

        private ComboBox AddChoice(Control parent, string[] items, int x, int y, int width, int selected,
            Action<int> apply)
        {
            ComboBox combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Items.AddRange(items);
            combo.SelectedIndex = selected;
            combo.Location = new Point(x, y);
            combo.Size = new Size(width, 23);
            combo.SelectedIndexChanged += delegate
            {
                if (_loading) return;
                if (apply != null) apply(combo.SelectedIndex);
                ApplyChanged();
            };
            parent.Controls.Add(combo);
            return combo;
        }

        private RadioButton AddRadio(Control parent, string text, int x, int y, int width, string hotkey)
        {
            RadioButton radio = new RadioButton();
            radio.Text = text;
            radio.Location = new Point(x, y);
            radio.Size = new Size(width, 21);
            radio.CheckedChanged += delegate
            {
                if (_loading || !radio.Checked) return;
                Settings.Hotkey = hotkey;
                ApplyChanged();
            };
            parent.Controls.Add(radio);
            return radio;
        }

        private CheckBox AddCheck(Control parent, string text, int x, int y, int width,
            bool supported, Action<bool> apply)
        {
            CheckBox box = new CheckBox();
            box.Text = text;
            box.Location = new Point(x, y);
            box.Size = new Size(width, 21);
            box.Enabled = supported;
            box.CheckedChanged += delegate
            {
                if (_loading) return;
                if (apply != null) apply(box.Checked);
                ApplyChanged();
            };
            parent.Controls.Add(box);
            return box;
        }

        private void AddButton(Control parent, string text, int x, int y, int width, int height,
            EventHandler onClick, bool supported)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new Point(x, y);
            button.Size = new Size(width, height);
            button.Enabled = supported;
            if (onClick != null) button.Click += onClick;
            parent.Controls.Add(button);
        }

        private void ApplyChanged()
        {
            if (_settingsChanged != null) _settingsChanged();
        }

        private void SetAutoStart(bool enabled)
        {
            bool changed = enabled ? AutoStart.Enable() : AutoStart.Disable();
            bool actual = AutoStart.IsEnabled();

            if (!changed || actual != enabled)
            {
                _loading = true;
                _autoStart.Checked = actual;
                _loading = false;

                MessageBox.Show(this, "Không thay đổi được thiết lập khởi động cùng Windows.", "ThiTap",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadValues()
        {
            _loading = true;
            try
            {
                bool altZ = string.Equals(Settings.Hotkey, Settings.HotkeyAltZ, StringComparison.Ordinal);
                _hotkeyAltZ.Checked = altZ;
                _hotkeyCtrlShift.Checked = !altZ;
                _typingMethod.SelectedIndex = Settings.VniMode ? 1 : 0;

                _freeTyping.Checked = Settings.FreeTyping;
                _oldStyleTone.Checked = Settings.OldStyleTone;
                _spellCheck.Checked = Settings.SpellCheck;
                _autoRestore.Checked = Settings.AutoRestore;
                _clipboard.Checked = Settings.ClipboardMode;
                _showNotification.Checked = Settings.ShowNotification;
                _beepOnSwitch.Checked = Settings.BeepOnSwitch;
                _showDialogOnStart.Checked = Settings.ShowDialogOnStart;
                _autoStart.Checked = AutoStart.IsEnabled();
                _macroEnabled.Checked = Settings.MacroEnabled;
                _macroWhenOff.Checked = Settings.MacroWhenOff;
                _appFilter.Checked = Settings.AppFilter;
            }
            finally
            {
                _loading = false;
            }
        }

        private void ResetToDefaults()
        {
            Settings.ResetToDefaults();
            LoadValues();
            ApplyChanged();
        }

        private void ShowMacros()
        {
            if (_macroForm == null || _macroForm.IsDisposed) _macroForm = new MacroForm();
            if (_macroForm.Visible)
            {
                _macroForm.Activate();
                return;
            }
            _macroForm.Show(this);
        }

        private void ShowApps()
        {
            if (_appForm == null || _appForm.IsDisposed) _appForm = new AppFilterForm();
            if (_appForm.Visible)
            {
                _appForm.Activate();
                return;
            }
            _appForm.Show(this);
        }

        private void ShowHelp()
        {
            MessageBox.Show(this,
                "Kiểu gõ Telex:\n" +
                "  s = sắc, f = huyền, r = hỏi, x = ngã, j = nặng\n" +
                "  z = bỏ dấu vừa đặt\n" +
                "  aa = â, ee = ê, oo = ô, w = ư / ơ / ă, dd = đ\n" +
                "  uo + w = ươ (nguowif → người)\n\n" +
                "Kiểu gõ VNI:\n" +
                "  1 = sắc, 2 = huyền, 3 = hỏi, 4 = ngã, 5 = nặng\n" +
                "  0 = bỏ dấu\n" +
                "  6 = â / ê / ô, 7 = ư / ơ, 8 = ă, 9 = đ\n" +
                "  ví dụ: tie6ng1 → tiếng, d9u7o7ng2 → đường\n\n" +
                "Chuyển tiếng Việt / English:\n" +
                "  Ctrl + Shift hoặc Alt + Z (chọn ở trên)\n" +
                "  hoặc bấm chuột trái vào biểu tượng ở khay hệ thống.\n\n" +
                "Gõ tắt: khai báo cặp khoá = nội dung trong bảng gõ tắt,\n" +
                "  gõ khoá rồi bấm dấu cách để thay thế (ví dụ vn → Việt Nam).",
                "Hướng dẫn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAbout()
        {
            MessageBox.Show(this,
                "ThiTap 1.0\nBộ gõ tiếng Việt cá nhân cho Windows.\n\n" +
                "Tác giả: Ông Thi\n" +
                "Bảng mã: Unicode dựng sẵn\n" +
                "Kiểu gõ: " + (Settings.VniMode ? "VNI" : "Telex") + "\n\n" +
                "Cài đặt lưu tại:\n" + Settings.SettingsPath,
                "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>Nút X chỉ ẩn hộp thoại; chương trình vẫn chạy dưới khay hệ thống.</summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            base.OnFormClosing(e);
        }
    }
}
