using System;
using System.Threading;
using System.Windows.Forms;

namespace ThiTap
{
    internal static class Program
    {
        private const string MutexName = "ThiTap.Ime.SingleInstance";

        [STAThread]
        private static void Main(string[] args)
        {
            bool created;
            using (Mutex mutex = new Mutex(true, MutexName, out created))
            {
                if (!created)
                {
                    MessageBox.Show("ThiTap đang chạy sẵn ở khay hệ thống.", "ThiTap",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // "ThiTap.exe --config": mở thẳng hộp thoại cấu hình khi khởi động.
                bool showConfig = Array.IndexOf(args, "--config") >= 0;
                Application.Run(new TrayApp(showConfig));

                GC.KeepAlive(mutex);
            }
        }
    }
}
