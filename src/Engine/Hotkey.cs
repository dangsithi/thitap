using System;

namespace ThiTap.Engine
{
    /// <summary>
    /// Tổ hợp phím chuyển chế độ gõ, lưu trong settings.ini dưới dạng chữ thường ngăn bằng
    /// dấu cộng: "ctrl+shift", "alt+z", "ctrl+oem3" (dấu `). Tổ hợp chỉ gồm phím bổ trợ thì
    /// bấm lúc các phím đó cùng xuống; có phím chính thì phím chính bị nuốt luôn để ứng dụng
    /// đang nhận phím không thấy.
    /// </summary>
    internal struct Hotkey
    {
        public const int VkShift = 0x10;
        public const int VkControl = 0x11;
        public const int VkMenu = 0x12;
        public const int VkLShift = 0xA0;
        public const int VkRShift = 0xA1;
        public const int VkLControl = 0xA2;
        public const int VkRControl = 0xA3;
        public const int VkLMenu = 0xA4;
        public const int VkRMenu = 0xA5;
        public const int VkLWin = 0x5B;
        public const int VkRWin = 0x5C;

        /// <summary>Mã của các phím có tên riêng; phím chữ, số, F1-F24, numpad đặt tên theo quy tắc.</summary>
        private static readonly int[] NamedCodes =
        {
            0x08, 0x09, 0x0D, 0x13, 0x14, 0x1B, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26,
            0x27, 0x28, 0x2D, 0x2E, 0x5B, 0x5C, 0x5D, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
            0x90, 0x91, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF, 0xC0, 0xDB, 0xDC, 0xDD, 0xDE
        };

        private static readonly string[] NamedTokens =
        {
            "back", "tab", "enter", "pause", "capslock", "esc", "space", "pageup", "pagedown",
            "end", "home", "left", "up", "right", "down", "insert", "delete", "lwin", "rwin",
            "apps", "multiply", "add", "separator", "subtract", "decimal", "divide", "numlock",
            "scroll", "oem1", "oemplus", "oemcomma", "oemminus", "oemperiod", "oem2", "oem3",
            "oem4", "oem5", "oem6", "oem7"
        };

        private static readonly string[] NamedLabels =
        {
            "Back", "Tab", "Enter", "Pause", "CapsLock", "Esc", "Space", "PgUp", "PgDn",
            "End", "Home", "\u2190", "\u2191", "\u2192", "\u2193", "Ins", "Del", "Win", "Win",
            "Menu", "*", "+", "", "-", ".", "/", "NumLock", "Scroll", ";", "=", ",", "-", ".",
            "/", "`", "[", "\\", "]", "'"
        };

        public bool Ctrl;
        public bool Alt;
        public bool Shift;
        public bool Win;

        /// <summary>Mã phím chính; 0 nghĩa là tổ hợp chỉ gồm các phím bổ trợ.</summary>
        public int Key;

        /// <summary>Tổ hợp mặc định của ThiTap (CTRL + SHIFT).</summary>
        public static Hotkey Default
        {
            get
            {
                Hotkey hotkey = new Hotkey();
                hotkey.Ctrl = true;
                hotkey.Shift = true;
                return hotkey;
            }
        }

        public bool HasKey
        {
            get { return Key != 0; }
        }

        public int ModifierCount
        {
            get
            {
                int count = 0;
                if (Ctrl) count++;
                if (Alt) count++;
                if (Shift) count++;
                if (Win) count++;
                return count;
            }
        }

        public bool IsEmpty
        {
            get { return ModifierCount == 0 && Key == 0; }
        }

        /// <summary>Đọc giá trị trong settings.ini; sai cú pháp thì trả về tổ hợp mặc định.</summary>
        public static Hotkey Parse(string text)
        {
            Hotkey hotkey = new Hotkey();
            if (string.IsNullOrEmpty(text)) return Default;

            // Bản cũ ghi "ctrl_shift" / "alt_z": dấu gạch dưới cũng là dấu ngăn.
            string[] parts = text.Replace('_', '+').Split('+');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim().ToLowerInvariant();
                if (part.Length == 0) continue;

                switch (part)
                {
                    case "ctrl":
                    case "control":
                        hotkey.Ctrl = true;
                        break;
                    case "alt":
                        hotkey.Alt = true;
                        break;
                    case "shift":
                        hotkey.Shift = true;
                        break;
                    case "win":
                        hotkey.Win = true;
                        break;
                    default:
                        int key;
                        if (TryParseKey(part, out key)) hotkey.Key = key;
                        break;
                }
            }

            return hotkey.IsEmpty ? Default : hotkey;
        }

        /// <summary>Chuỗi để ghi vào settings.ini, ví dụ "ctrl+shift" hoặc "ctrl+oem3".</summary>
        public string Serialize()
        {
            string text = string.Empty;
            if (Ctrl) text += "ctrl+";
            if (Alt) text += "alt+";
            if (Shift) text += "shift+";
            if (Win) text += "win+";
            if (Key != 0) text += Token(Key);
            return text.Length > 0 ? text.TrimEnd('+') : Default.Serialize();
        }

        /// <summary>Chuỗi hiển thị cho người dùng, ví dụ "Ctrl + Shift" hoặc "Ctrl + `".</summary>
        public string Describe()
        {
            string text = string.Empty;
            if (Ctrl) text = Join(text, "Ctrl");
            if (Alt) text = Join(text, "Alt");
            if (Shift) text = Join(text, "Shift");
            if (Win) text = Join(text, "Win");
            if (Key != 0) text = Join(text, Label(Key));
            return text.Length > 0 ? text : Default.Describe();
        }

        private static string Join(string text, string part)
        {
            return text.Length == 0 ? part : text + " + " + part;
        }

        /// <summary>
        /// Tổ hợp vừa được bấm: đúng các phím bổ trợ đang giữ và (nếu có) đúng phím chính.
        /// Phím bổ trợ thừa thì không tính, để Ctrl + Shift + A không kích hoạt Ctrl + Shift.
        /// </summary>
        public bool Matched(bool ctrl, bool alt, bool shift, bool win, int virtualKey)
        {
            if (ctrl != Ctrl || alt != Alt || shift != Shift || win != Win) return false;

            if (Key == 0) return IsRequiredModifier(virtualKey);
            return SameKey(virtualKey, Key);
        }

        /// <summary>Phím vừa xuống có phải phím bổ trợ nằm trong tổ hợp không.</summary>
        /// <remarks>
        /// Ctrl không bao giờ là phím kích hoạt: người dùng hay giữ Shift rồi bấm Ctrl + mũi tên
        /// để chọn từ, nếu Ctrl cũng kích hoạt thì mỗi lần như vậy lại đổi chế độ gõ.
        /// </remarks>
        private bool IsRequiredModifier(int virtualKey)
        {
            switch (virtualKey)
            {
                case VkShift:
                case VkLShift:
                case VkRShift:
                    return Shift;
                case VkControl:
                case VkLControl:
                case VkRControl:
                    return false;
                case VkMenu:
                case VkLMenu:
                case VkRMenu:
                    return Alt;
                case VkLWin:
                case VkRWin:
                    return Win;
                default:
                    return false;
            }
        }

        /// <summary>Hook báo mã riêng của phím trái/phải, cài đặt có thể ghi mã chung.</summary>
        private static bool SameKey(int actual, int wanted)
        {
            if (actual == wanted) return true;
            if (wanted == VkControl && (actual == VkLControl || actual == VkRControl)) return true;
            if (wanted == VkShift && (actual == VkLShift || actual == VkRShift)) return true;
            if (wanted == VkMenu && (actual == VkLMenu || actual == VkRMenu)) return true;
            return false;
        }

        /// <summary>Phím bổ trợ (Ctrl, Shift, Alt, Win) hay phím thường.</summary>
        public static bool IsModifier(int virtualKey)
        {
            switch (virtualKey)
            {
                case VkShift:
                case VkLShift:
                case VkRShift:
                case VkControl:
                case VkLControl:
                case VkRControl:
                case VkMenu:
                case VkLMenu:
                case VkRMenu:
                case VkLWin:
                case VkRWin:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Tên phím dùng trong settings.ini.</summary>
        public static string Token(int key)
        {
            if (key >= 0x41 && key <= 0x5A) return ((char)('a' + (key - 0x41))).ToString();
            if (key >= 0x30 && key <= 0x39) return ((char)key).ToString();
            if (key >= 0x70 && key <= 0x87) return "f" + (key - 0x6F);
            if (key >= 0x60 && key <= 0x69) return "num" + (key - 0x60);

            for (int i = 0; i < NamedCodes.Length; i++)
            {
                if (NamedCodes[i] == key) return NamedTokens[i];
            }
            return "vk" + key.ToString("x");
        }

        /// <summary>Tên phím để hiển thị trong hộp thoại cấu hình.</summary>
        public static string Label(int key)
        {
            if (key >= 0x41 && key <= 0x5A) return ((char)key).ToString();
            if (key >= 0x30 && key <= 0x39) return ((char)key).ToString();
            if (key >= 0x70 && key <= 0x87) return "F" + (key - 0x6F);

            for (int i = 0; i < NamedCodes.Length; i++)
            {
                if (NamedCodes[i] == key) return NamedLabels[i];
            }
            return Token(key).ToUpperInvariant();
        }

        private static bool TryParseKey(string token, out int key)
        {
            key = 0;
            if (token.Length == 0) return false;

            if (token.Length == 1)
            {
                char ch = token[0];
                if (ch >= 'a' && ch <= 'z')
                {
                    key = 0x41 + (ch - 'a');
                    return true;
                }
                if (ch >= '0' && ch <= '9')
                {
                    key = ch;
                    return true;
                }
                return false;
            }

            if (token[0] == 'f' && token.Length <= 3)
            {
                int number;
                if (int.TryParse(token.Substring(1), out number) && number >= 1 && number <= 24)
                {
                    key = 0x6F + number;
                    return true;
                }
            }

            if (token.Length == 4 && token.StartsWith("num", StringComparison.Ordinal) && token[3] >= '0' && token[3] <= '9')
            {
                key = 0x60 + (token[3] - '0');
                return true;
            }

            for (int i = 0; i < NamedTokens.Length; i++)
            {
                if (string.Equals(NamedTokens[i], token, StringComparison.Ordinal))
                {
                    key = NamedCodes[i];
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Hộp thoại cấu hình đang chờ bắt tổ hợp phím mới: hook bàn phím tạm bỏ qua phím tắt,
    /// nếu không thì chính tổ hợp đang bấm sẽ đổi chế độ gõ giữa lúc học phím.
    /// </summary>
    internal static class HotkeyCapture
    {
        public static volatile bool Active = false;
    }
}
