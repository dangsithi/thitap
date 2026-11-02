using System;
using System.Drawing;
using System.Windows.Forms;

namespace ThiTap
{
    /// <summary>
    /// Hộp thoại cấu hình giao diện tối: nền #202020, nội dung chia thành các card #2B2B2B
    /// bo góc, ô nhập lõm tối hơn, nút chính màu accent. Mỗi thay đổi được lưu và áp dụng
    /// ngay, nên nút "Đóng" chỉ để ẩn hộp thoại.
    /// </summary>
    internal sealed class ConfigForm : Form
    {
        /// <summary>Phiên bản hiển thị ở thanh tiêu đề và hộp thoại Thông tin.</summary>
        public const string Version = "1.3";

        private readonly Action _settingsChanged;
        private readonly Action _exitApp;

        private DarkComboBox _typingMethod;
        private DarkRadio _hotkeyCtrlShift;
        private DarkRadio _hotkeyAltZ;
        private DarkCheckBox _oldStyleTone;
        private DarkCheckBox _spellCheck;
        private DarkCheckBox _beepOnSwitch;
        private DarkCheckBox _showDialogOnStart;
        private DarkCheckBox _autoStart;
        private DarkCheckBox _clipboard;
        private DarkCheckBox _autoRestore;
        private DarkCheckBox _macroEnabled;
        private DarkCheckBox _macroWhenOff;
        private DarkCheckBox _appFilter;
        private MacroForm _macroForm;
        private AppFilterForm _appForm;

        public ConfigForm(Action settingsChanged, Action exitApp)
        {
            _settingsChanged = settingsChanged;
            _exitApp = exitApp;

            Text = "ThiTap " + Version;
            ClientSize = new Size(560, 610);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.None;

            Icon icon = IconDesign.CreateIcon(32, true);
            Icon = icon;

            Theme.Apply(this);

            BuildUi();
            LoadValues();

            // Không để nút đầu tiên nhận tiêu điểm khi mở (viền accent trông như đang chọn).
            ActiveControl = null;
        }

        private void BuildUi()
        {
            const int left = Ui.Pad;
            const int width = 528;
            const int right = left + width;

            Ui.Title(this, "ThiTap", left, 10, 220);
            Ui.Note(this, "Bộ gõ tiếng Việt · bảng mã Unicode dựng sẵn", left, 40, 300);

            Ui.Button(this, "Thu nhỏ", right - 200, 18, 96, 30, delegate { Hide(); });
            Ui.Button(this, "Kết thúc", right - 96, 18, 96, 30, delegate
            {
                if (_exitApp != null) _exitApp();
            });

            CardPanel controlCard = Ui.Card(this, "Điều khiển", left, 68, width, 144);

            Ui.Label(controlCard, "Bảng mã", 16, 46, 60);
            Ui.Combo(controlCard, new string[] { "Unicode" }, 80, 44, 132, 0, null, false);

            Ui.Label(controlCard, "Kiểu gõ", 232, 46, 54);
            _typingMethod = Ui.Combo(controlCard, new string[] { "Telex", "VNI" }, 292, 44, 118,
                Settings.VniMode ? 1 : 0, delegate(int index)
                {
                    Settings.InputMethod = index == 1 ? Settings.InputVni : Settings.InputTelex;
                    ApplyChanged();
                }, true);
            Ui.Button(controlCard, "...", 418, 44, 34, 24, null).Enabled = false;

            Ui.Label(controlCard, "Phím chuyển", 16, 86, 82);
            bool altZ = string.Equals(Settings.Hotkey, Settings.HotkeyAltZ, StringComparison.Ordinal);
            _hotkeyCtrlShift = Ui.Radio(controlCard, "CTRL + SHIFT", 102, 85, 108, !altZ,
                delegate { SelectHotkey(Settings.HotkeyCtrlShift); });
            _hotkeyAltZ = Ui.Radio(controlCard, "ALT + Z", 214, 85, 90, altZ,
                delegate { SelectHotkey(Settings.HotkeyAltZ); });

            CardPanel optionCard = Ui.Card(this, "Tùy chọn", left, 224, width, 138);

            _oldStyleTone = Ui.Check(optionCard, "Đặt dấu oà, uý (thay vì oá, úy)", 16, 44, 240, true,
                delegate(bool value) { Settings.OldStyleTone = value; ApplyChanged(); });
            _clipboard = Ui.Check(optionCard, "Luôn dùng clipboard cho Unicode", 16, 72, 240, true,
                delegate(bool value) { Settings.ClipboardMode = value; ApplyChanged(); });

            _spellCheck = Ui.Check(optionCard, "Bật kiểm tra chính tả", 272, 44, 240, true,
                delegate(bool value) { Settings.SpellCheck = value; ApplyChanged(); });
            _autoRestore = Ui.Check(optionCard, "Tự khôi phục phím tiếng Anh", 272, 72, 240, true,
                delegate(bool value) { Settings.AutoRestore = value; ApplyChanged(); });
            _beepOnSwitch = Ui.Check(optionCard, "Bật chuông khi chuyển ngôn ngữ", 272, 100, 240, true,
                delegate(bool value) { Settings.BeepOnSwitch = value; ApplyChanged(); });

            CardPanel macroCard = Ui.Card(this, "Gõ tắt", left, 374, 240, 146);
            _macroEnabled = Ui.Check(macroCard, "Cho phép gõ tắt", 16, 44, 208, true,
                delegate(bool value) { Settings.MacroEnabled = value; ApplyChanged(); });
            _macroWhenOff = Ui.Check(macroCard, "Gõ tắt cả khi tắt tiếng Việt", 16, 72, 208, true,
                delegate(bool value) { Settings.MacroWhenOff = value; ApplyChanged(); });
            Ui.Button(macroCard, "Bảng gõ tắt...", 16, 100, 150, 30, delegate { ShowMacros(); });

            CardPanel systemCard = Ui.Card(this, "Hệ thống", left + 256, 374, 272, 146);
            _showDialogOnStart = Ui.Check(systemCard, "Mở hội thoại này khi khởi động", 16, 44, 240, true,
                delegate(bool value) { Settings.ShowDialogOnStart = value; ApplyChanged(); });
            _autoStart = Ui.Check(systemCard, "Khởi động cùng Windows", 16, 72, 240, true, SetAutoStart);
            _appFilter = Ui.Check(systemCard, "Bật/tắt theo ứng dụng", 16, 100, 170, true,
                delegate(bool value) { Settings.AppFilter = value; ApplyChanged(); });
            Ui.Button(systemCard, "Chọn...", 194, 98, 62, 30, delegate { ShowApps(); });

            Ui.Note(this, "Bảng mã: chỉ Unicode. Các mục bị làm mờ chưa có trong bản này.",
                left, 534, width);

            Ui.Button(this, "Hướng dẫn", left, 562, 104, 32, delegate { ShowHelp(); });
            Ui.Button(this, "Thông tin", left + 112, 562, 96, 32, delegate { ShowAbout(); });
            Ui.Button(this, "Mặc định", left + 216, 562, 96, 32, delegate { ResetToDefaults(); });
            Ui.PrimaryButton(this, "Đóng", right - 104, 562, 104, 32, delegate { Hide(); });
        }

        private void SelectHotkey(string hotkey)
        {
            Settings.Hotkey = hotkey;

            // Nút chọn không còn tự loại trừ nhau như RadioButton, nên đồng bộ cả hai.
            Ui.Loading = true;
            _hotkeyCtrlShift.Checked = string.Equals(hotkey, Settings.HotkeyCtrlShift, StringComparison.Ordinal);
            _hotkeyAltZ.Checked = string.Equals(hotkey, Settings.HotkeyAltZ, StringComparison.Ordinal);
            Ui.Loading = false;

            ApplyChanged();
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
                Ui.Loading = true;
                _autoStart.Checked = actual;
                Ui.Loading = false;

                DarkBox.Warn(this, "Khởi động cùng Windows",
                    "Không thay đổi được thiết lập khởi động cùng Windows.");
                return;
            }

            ApplyChanged();
        }

        private void LoadValues()
        {
            Ui.Loading = true;
            try
            {
                bool altZ = string.Equals(Settings.Hotkey, Settings.HotkeyAltZ, StringComparison.Ordinal);
                _hotkeyAltZ.Checked = altZ;
                _hotkeyCtrlShift.Checked = !altZ;
                _typingMethod.SelectedIndex = Settings.VniMode ? 1 : 0;

                _oldStyleTone.Checked = Settings.OldStyleTone;
                _spellCheck.Checked = Settings.SpellCheck;
                _autoRestore.Checked = Settings.AutoRestore;
                _clipboard.Checked = Settings.ClipboardMode;
                _beepOnSwitch.Checked = Settings.BeepOnSwitch;
                _showDialogOnStart.Checked = Settings.ShowDialogOnStart;
                _autoStart.Checked = AutoStart.IsEnabled();
                _macroEnabled.Checked = Settings.MacroEnabled;
                _macroWhenOff.Checked = Settings.MacroWhenOff;
                _appFilter.Checked = Settings.AppFilter;
            }
            finally
            {
                Ui.Loading = false;
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
            DarkBox.Info(this, "Hướng dẫn gõ",
                "Kiểu gõ Telex:\n" +
                "s = sắc, f = huyền, r = hỏi, x = ngã, j = nặng, z = bỏ dấu vừa đặt\n" +
                "aa = â, ee = ê, oo = ô, w = ư / ơ / ă, dd = đ, [ = ư, ] = ơ\n" +
                "ww = w (bấm w hai lần khi muốn giữ đúng chữ w)\n" +
                "uo + w = ươ (nguowif → người)\n\n" +
                "Kiểu gõ VNI:\n" +
                "1 = sắc, 2 = huyền, 3 = hỏi, 4 = ngã, 5 = nặng, 0 = bỏ dấu\n" +
                "6 = â / ê / ô, 7 = ư / ơ, 8 = ă, 9 = đ\n" +
                "ví dụ: tie6ng1 → tiếng, d9u7o7ng2 → đường\n\n" +
                "Từ không phải âm tiết tiếng Việt thì không bị bỏ dấu: WWindowws → Windows.\n\n" +
                "Chuyển tiếng Việt / English:\n" +
                "Ctrl + Shift hoặc Alt + Z (chọn ở trên), hoặc bấm chuột trái vào biểu tượng\n" +
                "ở khay hệ thống.\n\n" +
                "Gõ tắt: khai báo cặp khoá = nội dung trong bảng gõ tắt, gõ khoá rồi bấm\n" +
                "dấu cách để thay thế (ví dụ vn → Việt Nam).");
        }

        private void ShowAbout()
        {
            DarkBox.Info(this, "Thông tin",
                "ThiTap " + Version + " — bộ gõ tiếng Việt cá nhân cho Windows.\n" +
                "Tác giả: Ông Thi\n" +
                "Bảng mã: Unicode dựng sẵn\n" +
                "Kiểu gõ: " + (Settings.VniMode ? "VNI" : "Telex") + "\n\n" +
                "Cài đặt lưu tại: " + Settings.SettingsPath);
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
