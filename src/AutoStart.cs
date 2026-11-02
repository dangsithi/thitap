using System;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ThiTap
{
    /// <summary>
    /// Đăng ký ThiTap vào khoá Run của HKEY_CURRENT_USER để tự chạy khi đăng nhập Windows.
    /// Ghi vào HKCU nên không cần quyền quản trị.
    /// </summary>
    internal static class AutoStart
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "ThiTap";

        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false))
                {
                    return key != null && key.GetValue(ValueName) != null;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (SecurityException)
            {
            }
            return false;
        }

        /// <summary>Thêm (hoặc cập nhật đường dẫn) mục khởi động cùng Windows.</summary>
        public static bool Enable()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (key == null) return false;
                    // Đường dẫn có thể chứa dấu cách nên phải đặt trong ngoặc kép.
                    key.SetValue(ValueName, "\"" + Application.ExecutablePath + "\"", RegistryValueKind.String);
                    return true;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (SecurityException)
            {
            }
            return false;
        }

        public static bool Disable()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true))
                {
                    if (key == null) return true;
                    key.DeleteValue(ValueName, false);
                    return true;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (SecurityException)
            {
            }
            return false;
        }
    }
}
