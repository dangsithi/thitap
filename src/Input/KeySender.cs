using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ThiTap.Input
{
    /// <summary>Gửi phím bằng SendInput: xoá ký tự và gõ chữ Unicode vào ứng dụng đang focus.</summary>
    internal static class KeySender
    {
        /// <summary>Dấu nhận dạng sự kiện do chính ThiTap sinh ra, để hook bỏ qua.</summary>
        public const uint Marker = 0x54484954; // "THIT"

        private const uint InputKeyboard = 1;
        private const uint KeyEventUnicode = 0x0004;
        private const uint KeyEventKeyUp = 0x0002;
        private const ushort VkBack = 0x08;
        private const ushort VkControl = 0x11;
        private const ushort VkV = 0x56;

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseInput
        {
            public int Dx;
            public int Dy;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardInput
        {
            public ushort VirtualKey;
            public ushort ScanCode;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HardwareInput
        {
            public uint Msg;
            public ushort ParamLow;
            public ushort ParamHigh;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MouseInput Mouse;
            [FieldOffset(0)] public KeyboardInput Keyboard;
            [FieldOffset(0)] public HardwareInput Hardware;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Input
        {
            public uint Type;
            public InputUnion Data;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint count, Input[] inputs, int size);

        /// <summary>Dán nội dung qua clipboard (Ctrl + V) thay vì gõ từng ký tự Unicode.</summary>
        public static bool UseClipboard { get; set; }

        /// <summary>Xoá <paramref name="backspaces"/> ký tự rồi gõ <paramref name="text"/>.</summary>
        public static void Replace(int backspaces, string text)
        {
            if (backspaces <= 0 && string.IsNullOrEmpty(text)) return;

            if (UseClipboard && !string.IsNullOrEmpty(text) && TryPaste(backspaces, text)) return;

            List<Input> events = new List<Input>();

            for (int i = 0; i < backspaces; i++)
            {
                events.Add(Keyboard(VkBack, 0));
                events.Add(Keyboard(VkBack, KeyEventKeyUp));
            }

            if (!string.IsNullOrEmpty(text))
            {
                for (int i = 0; i < text.Length; i++)
                {
                    // Ký tự ngoài BMP phải gửi theo cặp surrogate; tiếng Việt luôn nằm trong BMP.
                    if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    {
                        events.Add(Keyboard(0, KeyEventUnicode, text[i]));
                        events.Add(Keyboard(0, KeyEventUnicode | KeyEventKeyUp, text[i]));
                        events.Add(Keyboard(0, KeyEventUnicode, text[i + 1]));
                        events.Add(Keyboard(0, KeyEventUnicode | KeyEventKeyUp, text[i + 1]));
                        i++;
                        continue;
                    }

                    events.Add(Keyboard(0, KeyEventUnicode, text[i]));
                    events.Add(Keyboard(0, KeyEventUnicode | KeyEventKeyUp, text[i]));
                }
            }

            Input[] buffer = events.ToArray();
            SendInput((uint)buffer.Length, buffer, Marshal.SizeOf(typeof(Input)));
        }

        private static Input Keyboard(ushort virtualKey, uint flags, char scan = '\0')
        {
            Input item = new Input();
            item.Type = InputKeyboard;
            item.Data.Keyboard.VirtualKey = virtualKey;
            item.Data.Keyboard.ScanCode = scan;
            item.Data.Keyboard.Flags = flags;
            item.Data.Keyboard.Time = 0;
            item.Data.Keyboard.ExtraInfo = new IntPtr(unchecked((int)Marker));
            return item;
        }

        /// <summary>Đưa chữ vào clipboard rồi gửi Ctrl + V. Trả về false nếu không đặt được clipboard.</summary>
        private static bool TryPaste(int backspaces, string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
                return false;
            }

            List<Input> events = new List<Input>();
            for (int i = 0; i < backspaces; i++)
            {
                events.Add(Keyboard(VkBack, 0));
                events.Add(Keyboard(VkBack, KeyEventKeyUp));
            }

            events.Add(Keyboard(VkControl, 0));
            events.Add(Keyboard(VkV, 0));
            events.Add(Keyboard(VkV, KeyEventKeyUp));
            events.Add(Keyboard(VkControl, KeyEventKeyUp));

            Input[] buffer = events.ToArray();
            SendInput((uint)buffer.Length, buffer, Marshal.SizeOf(typeof(Input)));
            return true;
        }
    }
}
