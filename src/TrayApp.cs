using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using ThiTap.Engine;
using ThiTap.Input;

namespace ThiTap
{
    /// <summary>Ứng dụng chạy dưới khay hệ thống: bật/tắt tiếng Việt, mở hộp thoại cấu hình.</summary>
    internal sealed class TrayApp : ApplicationContext
    {
        private readonly TelexEngine _engine;
        private readonly KeyboardHook _hook;
        private readonly NotifyIcon _tray;
        private readonly ToolStripMenuItem _toggleItem;
        private readonly ToolStripMenuItem _autoStartItem;
        private ConfigForm _config;
        private Icon _icon;

        public TrayApp(bool showConfig)
        {
            _engine = new TelexEngine();
            ApplySettings();

            _toggleItem = new ToolStripMenuItem("Tiếng Việt (Telex)");
            _toggleItem.Click += delegate { SetEnabled(!_engine.Enabled); };

            ToolStripMenuItem configItem = new ToolStripMenuItem("Cấu hình...");
            configItem.Click += delegate { ShowConfig(); };

            _autoStartItem = new ToolStripMenuItem("Khởi động cùng Windows");
            _autoStartItem.Click += delegate { SetAutoStart(!AutoStart.IsEnabled()); };

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Thoát");
            exitItem.Click += delegate { ExitApp(); };

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add(_toggleItem);
            menu.Items.Add(configItem);
            menu.Items.Add(_autoStartItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            _tray = new NotifyIcon();
            _tray.ContextMenuStrip = menu;
            _tray.MouseClick += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left) SetEnabled(!_engine.Enabled);
            };
            _tray.Visible = true;

            _autoStartItem.Checked = AutoStart.IsEnabled();
            if (_autoStartItem.Checked)
            {
                // Ghi lại đường dẫn phòng khi thư mục chứa ThiTap.exe đã đổi chỗ.
                AutoStart.Enable();
            }

            RefreshTray();

            _hook = new KeyboardHook(_engine, delegate { SetEnabled(!_engine.Enabled); });
            _hook.Install();

            if (showConfig || Settings.ShowDialogOnStart) ShowConfig();
        }

        /// <summary>Đọc cài đặt vào lõi gõ và các cờ dùng chung.</summary>
        private void ApplySettings()
        {
            _engine.Enabled = Settings.Enabled;
            _engine.SpellCheck = Settings.SpellCheck;
            _engine.Vni = Settings.VniMode;
            _engine.AutoRestoreEnglish = Settings.AutoRestore;
            _engine.MacroWhenDisabled = Settings.MacroWhenOff;
            _engine.MacroLookup = Settings.MacroEnabled ? new Func<string, string>(MacroStore.Lookup) : null;
            Vietnamese.OldStyleTone = Settings.OldStyleTone;
            KeySender.UseClipboard = Settings.ClipboardMode;
            _engine.ResetWord();
        }

        private void SetEnabled(bool enabled)
        {
            _engine.Enabled = enabled;
            _engine.ResetWord();
            Settings.Enabled = enabled;
            RefreshTray();

            if (Settings.BeepOnSwitch)
            {
                SystemSounds.Asterisk.Play();
            }
        }

        private void SetAutoStart(bool enabled)
        {
            bool ok = enabled ? AutoStart.Enable() : AutoStart.Disable();
            _autoStartItem.Checked = AutoStart.IsEnabled();

            if (!ok)
            {
                MessageBox.Show("Không thay đổi được thiết lập khởi động cùng Windows.", "ThiTap",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowConfig()
        {
            if (_config == null || _config.IsDisposed)
            {
                _config = new ConfigForm(
                    delegate { ApplySettings(); RefreshTray(); },
                    delegate { ExitApp(); });
            }

            _config.Show();
            if (_config.WindowState == FormWindowState.Minimized) _config.WindowState = FormWindowState.Normal;
            _config.Activate();
            _config.BringToFront();
        }

        private void RefreshTray()
        {
            bool vietnamese = _engine.Enabled;
            string method = Settings.VniMode ? "VNI" : "Telex";

            _toggleItem.Text = "Tiếng Việt (" + method + ")";
            _toggleItem.Checked = vietnamese;
            _tray.Text = vietnamese ? "ThiTap: Tiếng Việt (" + method + ")" : "ThiTap: English";

            Icon next = IconDesign.CreateIcon(16, vietnamese);
            _tray.Icon = next;
            if (_icon != null) _icon.Dispose();
            _icon = next;
        }

        private void ExitApp()
        {
            _hook.Dispose();
            _tray.Visible = false;
            _tray.Dispose();
            if (_icon != null) _icon.Dispose();
            if (_config != null) _config.Dispose();
            ExitThread();
        }
    }
}
