using System;
using System.Collections.Generic;
using System.IO;

namespace ThiTap
{
    /// <summary>
    /// Cài đặt dạng key=value trong file settings.ini nằm cạnh ThiTap.exe (dạng portable).
    /// Nếu thư mục chứa exe không ghi được thì lùi về %APPDATA%\ThiTap\settings.ini.
    /// Mỗi thay đổi được ghi ngay xuống đĩa; file không đọc/ghi được thì giữ giá trị mặc định.
    /// </summary>
    internal static class Settings
    {
        public const string KeyEnabled = "enabled";
        public const string KeyHotkey = "hotkey";
        public const string KeyFreeTyping = "freeTyping";
        public const string KeySpellCheck = "spellCheck";
        public const string KeyOldStyleTone = "oldStyleTone";
        public const string KeyShowNotification = "showNotification";
        public const string KeyBeepOnSwitch = "beepOnSwitch";
        public const string KeyShowDialogOnStart = "showDialogOnStart";
        public const string KeyMacroEnabled = "macroEnabled";
        public const string KeyMacroWhenOff = "macroWhenOff";
        public const string KeyAutoRestore = "autoRestore";
        public const string KeyClipboardMode = "clipboardMode";
        public const string KeyAppFilter = "appFilter";
        public const string KeyInputMethod = "inputMethod";

        public const string HotkeyCtrlShift = "ctrl_shift";
        public const string HotkeyAltZ = "alt_z";
        public const string InputTelex = "telex";
        public const string InputVni = "vni";

        private const string FileName = "settings.ini";

        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ThiTap");

        private static readonly Dictionary<string, string> Values =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static string _filePath;
        private static bool _loaded;

        /// <summary>Đường dẫn file cài đặt đang dùng, để hiển thị trong hộp thoại Thông tin.</summary>
        public static string SettingsPath
        {
            get
            {
                Load();
                return _filePath;
            }
        }

        /// <summary>Đọc file cài đặt; lần đầu chưa có thì tạo file mới cạnh exe.</summary>
        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;

            _filePath = ResolveFile();

            bool hasContent = false;
            if (File.Exists(_filePath))
            {
                Parse(_filePath);
                hasContent = Values.Count > 0;
            }

            // Chưa có nội dung: kế thừa file cũ ở %APPDATA% (bản trước đây) nếu khác chỗ.
            if (!hasContent)
            {
                string legacy = Path.Combine(AppDataFolder, FileName);
                if (!string.Equals(legacy, _filePath, StringComparison.OrdinalIgnoreCase) && File.Exists(legacy))
                {
                    Parse(legacy);
                }
            }

            FillMissingDefaults();
            Save();
        }

        /// <summary>
        /// Chọn chỗ lưu: cạnh exe nếu tạo được file, ngược lại là %APPDATA%.
        /// Việc tạo thử cũng chính là "sinh ra file cấu hình" ngay lần chạy đầu.
        /// </summary>
        private static string ResolveFile()
        {
            string portable = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
            if (File.Exists(portable)) return portable;

            if (CanCreate(portable)) return portable;

            try
            {
                Directory.CreateDirectory(AppDataFolder);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            return Path.Combine(AppDataFolder, FileName);
        }

        private static bool CanCreate(string path)
        {
            try
            {
                using (new FileStream(path, FileMode.Append, FileAccess.Write)) { }
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static void Parse(string path)
        {
            try
            {
                string[] lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;

                    int separator = line.IndexOf('=');
                    if (separator <= 0) continue;

                    string key = line.Substring(0, separator).Trim();
                    string value = line.Substring(separator + 1).Trim();
                    if (key.Length > 0) Values[key] = value;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static void ApplyDefaults()
        {
            Values.Clear();
            FillMissingDefaults();
        }

        /// <summary>Bổ sung các khoá còn thiếu bằng giá trị mặc định để file luôn đầy đủ.</summary>
        private static void FillMissingDefaults()
        {
            EnsureDefault(KeyEnabled, "1");
            EnsureDefault(KeyHotkey, HotkeyCtrlShift);
            EnsureDefault(KeyFreeTyping, "0");
            EnsureDefault(KeySpellCheck, "1");
            EnsureDefault(KeyOldStyleTone, "0");
            EnsureDefault(KeyShowNotification, "1");
            EnsureDefault(KeyBeepOnSwitch, "0");
            EnsureDefault(KeyShowDialogOnStart, "0");
            EnsureDefault(KeyMacroEnabled, "0");
            EnsureDefault(KeyMacroWhenOff, "0");
            EnsureDefault(KeyAutoRestore, "0");
            EnsureDefault(KeyClipboardMode, "0");
            EnsureDefault(KeyAppFilter, "0");
            EnsureDefault(KeyInputMethod, InputTelex);
        }

        private static void EnsureDefault(string key, string value)
        {
            if (!Values.ContainsKey(key)) Values[key] = value;
        }

        public static string GetString(string key, string fallback)
        {
            Load();
            string value;
            return Values.TryGetValue(key, out value) ? value : fallback;
        }

        public static bool GetBool(string key, bool fallback)
        {
            string value = GetString(key, fallback ? "1" : "0");
            return value != "0";
        }

        public static void SetString(string key, string value)
        {
            Load();
            Values[key] = value;
            Save();
        }

        public static void SetBool(string key, bool value)
        {
            SetString(key, value ? "1" : "0");
        }

        /// <summary>Đưa các khoá đã biết về mặc định (không đụng tới mục khởi động cùng Windows).</summary>
        public static void ResetToDefaults()
        {
            Load();
            ApplyDefaults();
            Save();
        }

        public static void Save()
        {
            if (_filePath == null) return;

            try
            {
                string[] values = new string[Values.Count];
                int index = 0;
                foreach (KeyValuePair<string, string> item in Values)
                {
                    values[index] = item.Key + "=" + item.Value;
                    index++;
                }
                Array.Sort(values, StringComparer.OrdinalIgnoreCase);

                List<string> lines = new List<string>(values.Length + 1);
                lines.Add("# ThiTap - cau hinh bo go tieng Viet (Telex/VNI). Sua tay duoc khi ung dung da tat.");
                lines.AddRange(values);

                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                File.WriteAllLines(_filePath, lines.ToArray());
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static bool Enabled
        {
            get { return GetBool(KeyEnabled, true); }
            set { SetBool(KeyEnabled, value); }
        }

        public static string Hotkey
        {
            get { return GetString(KeyHotkey, HotkeyCtrlShift); }
            set { SetString(KeyHotkey, value); }
        }

        public static bool FreeTyping
        {
            get { return GetBool(KeyFreeTyping, false); }
            set { SetBool(KeyFreeTyping, value); }
        }

        public static bool SpellCheck
        {
            get { return GetBool(KeySpellCheck, true); }
            set { SetBool(KeySpellCheck, value); }
        }

        public static bool OldStyleTone
        {
            get { return GetBool(KeyOldStyleTone, false); }
            set { SetBool(KeyOldStyleTone, value); }
        }

        public static bool ShowNotification
        {
            get { return GetBool(KeyShowNotification, true); }
            set { SetBool(KeyShowNotification, value); }
        }

        public static bool BeepOnSwitch
        {
            get { return GetBool(KeyBeepOnSwitch, false); }
            set { SetBool(KeyBeepOnSwitch, value); }
        }

        public static bool ShowDialogOnStart
        {
            get { return GetBool(KeyShowDialogOnStart, false); }
            set { SetBool(KeyShowDialogOnStart, value); }
        }

        /// <summary>Cho phép gõ tắt theo bảng macros.txt.</summary>
        public static bool MacroEnabled
        {
            get { return GetBool(KeyMacroEnabled, false); }
            set { SetBool(KeyMacroEnabled, value); }
        }

        /// <summary>Gõ tắt vẫn chạy khi đang ở chế độ tiếng Anh.</summary>
        public static bool MacroWhenOff
        {
            get { return GetBool(KeyMacroWhenOff, false); }
            set { SetBool(KeyMacroWhenOff, value); }
        }

        /// <summary>Khôi phục đúng phím đã bấm khi từ trông giống tiếng Anh.</summary>
        public static bool AutoRestore
        {
            get { return GetBool(KeyAutoRestore, false); }
            set { SetBool(KeyAutoRestore, value); }
        }

        /// <summary>Dán qua clipboard thay vì gõ từng ký tự Unicode.</summary>
        public static bool ClipboardMode
        {
            get { return GetBool(KeyClipboardMode, false); }
            set { SetBool(KeyClipboardMode, value); }
        }

        /// <summary>Tôn trọng danh sách ứng dụng tắt tiếng Việt trong apps.txt.</summary>
        public static bool AppFilter
        {
            get { return GetBool(KeyAppFilter, false); }
            set { SetBool(KeyAppFilter, value); }
        }

        /// <summary>Kiểu gõ: "telex" hoặc "vni".</summary>
        public static string InputMethod
        {
            get { return GetString(KeyInputMethod, InputTelex); }
            set { SetString(KeyInputMethod, value); }
        }

        public static bool VniMode
        {
            get { return string.Equals(InputMethod, InputVni, StringComparison.OrdinalIgnoreCase); }
        }
    }
}
