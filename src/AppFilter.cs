using System;
using System.Collections.Generic;
using System.IO;

namespace ThiTap
{
    /// <summary>
    /// Danh sách ứng dụng không dùng tiếng Việt, lưu trong apps.txt
    /// (cùng thư mục với settings.ini), mỗi dòng một tên tiến trình.
    /// </summary>
    internal static class AppFilter
    {
        private const string FileName = "apps.txt";
        private const string Header = "# ThiTap - cac ung dung TAT tieng Viet. Moi dong mot ten tien trinh, vi du: game.exe";

        private static readonly List<string> Items = new List<string>();
        private static bool _loaded;
        private static int _version;

        /// <summary>Tăng mỗi lần danh sách thay đổi, để hook biết cần đọc lại.</summary>
        public static int Version
        {
            get { return _version; }
        }

        public static string FilePath
        {
            get
            {
                Load();
                string folder = Path.GetDirectoryName(Settings.SettingsPath);
                return Path.Combine(folder, FileName);
            }
        }

        public static string[] All
        {
            get
            {
                Load();
                return Items.ToArray();
            }
        }

        public static bool IsDisabledFor(string processName)
        {
            if (string.IsNullOrEmpty(processName)) return false;
            Load();
            for (int i = 0; i < Items.Count; i++)
            {
                if (string.Equals(Items[i], processName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static void Set(string[] names)
        {
            Load();
            Items.Clear();
            if (names != null)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i] == null ? string.Empty : names[i].Trim();
                    if (name.Length == 0) continue;
                    if (Items.Contains(name)) continue;
                    Items.Add(name);
                }
            }
            Items.Sort(StringComparer.OrdinalIgnoreCase);
            _version++;
            Save();
        }

        private static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            Items.Clear();

            string path = Path.Combine(Path.GetDirectoryName(Settings.SettingsPath), FileName);
            try
            {
                if (!File.Exists(path))
                {
                    Save();
                    return;
                }

                string[] lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                    if (!Items.Contains(line)) Items.Add(line);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static void Save()
        {
            try
            {
                List<string> lines = new List<string>();
                lines.Add(Header);
                lines.AddRange(Items);

                string path = Path.Combine(Path.GetDirectoryName(Settings.SettingsPath), FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllLines(path, lines.ToArray());
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
