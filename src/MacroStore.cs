using System;
using System.Collections.Generic;
using System.IO;

namespace ThiTap
{
    /// <summary>
    /// Bảng gõ tắt lưu trong macros.txt (cùng thư mục với settings.ini).
    /// Mỗi dòng dạng "khoá=thay thế", dòng bắt đầu bằng # là chú thích.
    /// </summary>
    internal static class MacroStore
    {
        private const string FileName = "macros.txt";
        private const string Header = "# ThiTap - go tat. Moi dong: khoa=thay the (vi du: vn=Viet Nam)";

        internal struct Entry
        {
            public string Trigger;
            public string Replacement;

            public Entry(string trigger, string replacement)
            {
                Trigger = trigger;
                Replacement = replacement;
            }

            public override string ToString()
            {
                return Trigger + " = " + Replacement;
            }
        }

        private static readonly List<Entry> Items = new List<Entry>();
        private static bool _loaded;

        public static string FilePath
        {
            get
            {
                Load();
                string folder = Path.GetDirectoryName(Settings.SettingsPath);
                return Path.Combine(folder, FileName);
            }
        }

        /// <summary>Bản sao danh sách hiện tại để hộp thoại hiển thị.</summary>
        public static List<Entry> All
        {
            get
            {
                Load();
                return new List<Entry>(Items);
            }
        }

        /// <summary>Tra cứu theo đúng chữ đang hiển thị; trả null nếu không có.</summary>
        public static string Lookup(string word)
        {
            Load();
            for (int i = 0; i < Items.Count; i++)
            {
                if (string.Equals(Items[i].Trigger, word, StringComparison.OrdinalIgnoreCase))
                {
                    return Items[i].Replacement;
                }
            }
            return null;
        }

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            Items.Clear();

            string path = Path.Combine(Path.GetDirectoryName(Settings.SettingsPath), FileName);
            try
            {
                if (!File.Exists(path))
                {
                    SaveRaw(path);
                    return;
                }

                string[] lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;

                    int separator = line.IndexOf('=');
                    if (separator <= 0) continue;

                    string trigger = line.Substring(0, separator).Trim();
                    string replacement = line.Substring(separator + 1).Trim();
                    if (trigger.Length > 0 && replacement.Length > 0) Items.Add(new Entry(trigger, replacement));
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static void Add(string trigger, string replacement)
        {
            Load();
            Items.Add(new Entry(trigger.Trim(), replacement.Trim()));
            Save();
        }

        public static void Update(int index, string trigger, string replacement)
        {
            Load();
            if (index < 0 || index >= Items.Count) return;
            Items[index] = new Entry(trigger.Trim(), replacement.Trim());
            Save();
        }

        public static void Remove(int index)
        {
            Load();
            if (index < 0 || index >= Items.Count) return;
            Items.RemoveAt(index);
            Save();
        }

        public static void Save()
        {
            Load();
            SaveRaw(FilePath);
        }

        private static void SaveRaw(string path)
        {
            try
            {
                List<string> lines = new List<string>();
                lines.Add(Header);
                for (int i = 0; i < Items.Count; i++)
                {
                    lines.Add(Items[i].Trigger + "=" + Items[i].Replacement);
                }

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
