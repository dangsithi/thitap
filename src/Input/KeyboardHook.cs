using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ThiTap.Engine;

namespace ThiTap.Input
{
    /// <summary>Bắt phím toàn hệ thống ở mức thấp (WH_KEYBOARD_LL) và đưa chữ cái cho lõi Telex.</summary>
    internal sealed class KeyboardHook : IDisposable
    {
        private const int WhKeyboardLl = 13;
        private const int WmKeyDown = 0x0100;
        private const int WmSysKeyDown = 0x0104;

        private const int VkShift = 0x10;
        private const int VkControl = 0x11;
        private const int VkMenu = 0x12;
        private const int VkCapital = 0x14;
        private const int VkBack = 0x08;
        private const int VkOpenBracket = 0xDB;
        private const int VkCloseBracket = 0xDD;
        private const int VkLShift = 0xA0;
        private const int VkRShift = 0xA1;
        private const int VkLControl = 0xA2;
        private const int VkRControl = 0xA3;
        private const int VkLMenu = 0xA4;
        private const int VkRMenu = 0xA5;
        private const int VkLWin = 0x5B;
        private const int VkRWin = 0x5C;

        [StructLayout(LayoutKind.Sequential)]
        private struct KbdLlHookStruct
        {
            public uint VirtualKey;
            public uint ScanCode;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int hookId, HookProc proc, IntPtr module, uint threadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentProcessId();

        private readonly TelexEngine _engine;
        private readonly Action _toggle;
        private readonly HookProc _proc;
        private IntPtr _hook = IntPtr.Zero;
        private uint _lastToggle;
        private string _hotkeyText;
        private Hotkey _hotkey;
        private IntPtr _filterWindow = IntPtr.Zero;
        private bool _filterAllowed = true;
        private int _filterVersion = -1;

        public KeyboardHook(TelexEngine engine, Action toggle)
        {
            _engine = engine;
            _toggle = toggle;
            _proc = HookCallback;
        }

        public void Install()
        {
            if (_hook != IntPtr.Zero) return;
            using (Process current = Process.GetCurrentProcess())
            using (ProcessModule module = current.MainModule)
            {
                _hook = SetWindowsHookEx(WhKeyboardLl, _proc, GetModuleHandle(module.ModuleName), 0);
            }
            if (_hook == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "Không cài được hook bàn phím (mã lỗi " + Marshal.GetLastWin32Error() + ").");
            }
        }

        public void Dispose()
        {
            if (_hook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && (wParam == (IntPtr)WmKeyDown || wParam == (IntPtr)WmSysKeyDown))
            {
                KbdLlHookStruct data = (KbdLlHookStruct)Marshal.PtrToStructure(lParam, typeof(KbdLlHookStruct));
                if (data.ExtraInfo != new IntPtr(unchecked((int)KeySender.Marker)))
                {
                    bool swallow;
                    try
                    {
                        swallow = HandleKey((int)data.VirtualKey);
                    }
                    catch (Exception)
                    {
                        // Lỗi trong hook callback sẽ làm treo cả chuỗi hook: bỏ qua phím thay vì ném ra.
                        _engine.ResetWord();
                        swallow = false;
                    }
                    if (swallow) return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hook, code, wParam, lParam);
        }

        private bool HandleKey(int virtualKey)
        {
            bool shift = IsDown(VkShift);
            bool control = IsDown(VkControl);
            bool alt = IsDown(VkMenu);
            bool win = IsDown(VkLWin) || IsDown(VkRWin);

            // Hook chạy trước khi Windows cập nhật trạng thái phím vừa bấm, nên chính phím
            // đang xử lý phải được tính là đang giữ. Nếu không, Ctrl + Shift không bao giờ khớp
            // vì lúc Shift xuống thì GetAsyncKeyState còn trả về "chưa bấm".
            if (virtualKey == VkShift || virtualKey == VkLShift || virtualKey == VkRShift)
            {
                shift = true;
            }
            else if (virtualKey == VkControl || virtualKey == VkLControl || virtualKey == VkRControl)
            {
                control = true;
            }
            else if (virtualKey == VkMenu || virtualKey == VkLMenu || virtualKey == VkRMenu)
            {
                alt = true;
            }
            else if (virtualKey == VkLWin || virtualKey == VkRWin)
            {
                win = true;
            }

            // Phím chuyển ngôn ngữ chạy cả khi con trỏ đang ở trong hộp thoại của ThiTap.
            // Lúc hộp thoại cấu hình đang chờ bắt tổ hợp mới thì tạm bỏ qua.
            if (!HotkeyCapture.Active)
            {
                Hotkey hotkey = CurrentHotkey();
                if (hotkey.Matched(control, alt, shift, win, virtualKey))
                {
                    ToggleIfReady();
                    return hotkey.HasKey;
                }
            }

            // Đang gõ trong chính hộp thoại của ThiTap thì để nguyên, không biến đổi gì.
            if (IsOwnWindowFocused())
            {
                _engine.ResetWord();
                return false;
            }

            if (virtualKey == VkLShift || virtualKey == VkRShift
                || virtualKey == VkLControl || virtualKey == VkRControl
                || virtualKey == VkLMenu || virtualKey == VkRMenu
                || virtualKey == VkCapital || virtualKey == VkLWin || virtualKey == VkRWin)
            {
                return false;
            }

            if (control || alt)
            {
                _engine.ResetWord();
                return false;
            }

            // Ứng dụng nằm trong danh sách tắt tiếng Việt: để phím đi qua nguyên vẹn.
            if (IsAppDisabled())
            {
                _engine.ResetWord();
                return false;
            }

            // Backspace: ứng dụng tự xoá một ký tự, chỉ cần bảo lõi gõ rút lại phím thô theo,
            // nếu xoá trắng luôn thì lần gõ tiếp theo phím dấu sẽ bị dính ra ngoài chữ ("hôji").
            if (virtualKey == VkBack)
            {
                _engine.ProcessBackspace();
                return false;
            }

            char keyChar;
            bool digit;
            if (!TryMapInput(virtualKey, out keyChar, out digit))
            {
                // Dấu cách, Enter, dấu câu... là lúc chốt từ: mở rộng gõ tắt hoặc khôi phục tiếng Anh.
                if (IsExpandBoundary(virtualKey))
                {
                    ExpandWord();
                }
                else
                {
                    _engine.ResetWord();
                }
                return false;
            }

            bool capsLock = (GetAsyncKeyState(VkCapital) & 1) != 0;
            bool upper = !digit && (shift != capsLock);

            KeyResult result = _engine.ProcessLetter(keyChar, upper);
            if (!result.Handled) return false;

            KeySender.Replace(result.Backspaces, result.Text);
            return true;
        }

        private void ToggleIfReady()
        {
            // Environment.TickCount quay vòng sau 49,7 ngày và có lúc trả về số âm, nên phải
            // tính hiệu bằng số không dấu; so sánh có dấu kiểu cũ làm phím tắt chết hẳn
            // sau khi máy chạy được khoảng 25 ngày.
            uint now = unchecked((uint)Environment.TickCount);
            if (unchecked(now - _lastToggle) < 400) return;
            _lastToggle = now;
            if (_toggle != null) _toggle();
        }

        /// <summary>
        /// Tổ hợp phím đang cài, đọc lại khi settings.ini đổi (hộp thoại cấu hình ghi ngay),
        /// nhưng không phải phân tích chuỗi ở mỗi lần gõ phím.
        /// </summary>
        private Hotkey CurrentHotkey()
        {
            string text = Settings.Hotkey;
            if (!string.Equals(text, _hotkeyText, StringComparison.Ordinal))
            {
                _hotkeyText = text;
                _hotkey = Hotkey.Parse(text);
            }
            return _hotkey;
        }

        /// <summary>Chốt từ hiện tại: thay bằng nội dung gõ tắt hoặc phím tiếng Anh nếu có.</summary>
        private void ExpandWord()
        {
            string replacement;
            int backspaces;
            if (_engine.TryExpand(out replacement, out backspaces))
            {
                KeySender.Replace(backspaces, replacement);
            }
            _engine.ResetWord();
        }

        /// <summary>Phím kết thúc từ (dấu cách, dấu câu, Enter...) thì mới xét gõ tắt / khôi phục.</summary>
        private static bool IsExpandBoundary(int virtualKey)
        {
            switch (virtualKey)
            {
                case 0x20: // Space
                case 0x0D: // Enter
                case 0x09: // Tab
                case 0x6C: // dấu phẩy
                case 0xBE: // dấu chấm
                case 0xBF: // dấu /
                case 0xBA: // dấu ;
                case 0xDE: // dấu '
                case 0xDC: // dấu \
                case 0xBB: // dấu =
                case 0xBD: // dấu -
                case 0x6B: // +
                case 0x6D: // -
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Tiến trình đang nhận phím có nằm trong danh sách tắt tiếng Việt không.</summary>
        private bool IsAppDisabled()
        {
            if (!Settings.AppFilter) return false;

            IntPtr window = GetForegroundWindow();
            if (window == _filterWindow && _filterVersion == AppFilter.Version) return !_filterAllowed;

            _filterWindow = window;
            _filterVersion = AppFilter.Version;
            _filterAllowed = true;

            uint processId;
            GetWindowThreadProcessId(window, out processId);
            if (processId != 0)
            {
                try
                {
                    using (Process process = Process.GetProcessById((int)processId))
                    {
                        string name = process.ProcessName;
                        if (AppFilter.IsDisabledFor(name) || AppFilter.IsDisabledFor(name + ".exe"))
                        {
                            _filterAllowed = false;
                        }
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            }

            return !_filterAllowed;
        }

        /// <summary>Cửa sổ đang nhận phím có thuộc chính tiến trình ThiTap không.</summary>
        private static bool IsOwnWindowFocused()
        {
            IntPtr window = GetForegroundWindow();
            if (window == IntPtr.Zero) return false;

            uint processId;
            GetWindowThreadProcessId(window, out processId);
            return processId == GetCurrentProcessId();
        }

        private static bool IsDown(int virtualKey)
        {
            return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
        }

        /// <summary>
        /// Chuyển phím thành ký tự cho lõi gõ: A-Z, và 0-9 khi dùng kiểu VNI
        /// (kèm Shift thì là ký tự khác nên để ứng dụng tự nhận).
        /// </summary>
        private bool TryMapInput(int virtualKey, out char keyChar, out bool digit)
        {
            digit = false;

            if (virtualKey >= 0x41 && virtualKey <= 0x5A)
            {
                keyChar = (char)('a' + (virtualKey - 0x41));
                return true;
            }

            if (_engine.Vni && virtualKey >= 0x30 && virtualKey <= 0x39 && !IsDown(VkShift))
            {
                keyChar = (char)('0' + (virtualKey - 0x30));
                digit = true;
                return true;
            }

            // Telex: "[" và "]" là phím gõ nhanh ư và ơ; kèm Shift thì vẫn là dấu câu.
            if (!_engine.Vni && !IsDown(VkShift)
                && (virtualKey == VkOpenBracket || virtualKey == VkCloseBracket))
            {
                keyChar = virtualKey == VkOpenBracket ? '[' : ']';
                return true;
            }

            keyChar = '\0';
            return false;
        }
    }
}
