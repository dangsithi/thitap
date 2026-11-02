using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ThiTap.Tools
{
    /// <summary>
    /// Sinh file .ico nhiều kích thước (16 → 256) từ thiết kế trong src\IconDesign.cs.
    /// Các cỡ nhỏ dùng DIB 32bpp (tương thích rộng), riêng 256 dùng PNG cho nhẹ.
    /// Chạy: MakeIcon.exe <đường dẫn .ico>
    /// </summary>
    internal static class MakeIcon
    {
        private static readonly int[] Sizes = { 16, 20, 24, 32, 48, 64, 128 };
        private const int LargeSize = 256;

        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            string output = args.Length > 0 ? args[0] : "ThiTap.ico";
            string folder = Path.GetDirectoryName(Path.GetFullPath(output));
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);

            List<int> widths = new List<int>(Sizes);
            widths.Add(LargeSize);

            List<byte[]> images = new List<byte[]>();
            for (int i = 0; i < widths.Count; i++)
            {
                images.Add(widths[i] >= LargeSize ? RenderPng(widths[i]) : RenderDib(widths[i]));
            }

            using (FileStream stream = new FileStream(output, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)0);            // reserved
                writer.Write((ushort)1);            // type: icon
                writer.Write((ushort)widths.Count); // số ảnh

                int offset = 6 + 16 * widths.Count;
                for (int i = 0; i < widths.Count; i++)
                {
                    int size = widths[i];
                    writer.Write((byte)(size >= 256 ? 0 : size)); // rộng (0 = 256)
                    writer.Write((byte)(size >= 256 ? 0 : size)); // cao
                    writer.Write((byte)0);                        // số màu bảng màu
                    writer.Write((byte)0);                        // reserved
                    writer.Write((ushort)1);                      // planes
                    writer.Write((ushort)32);                     // bit depth
                    writer.Write((uint)images[i].Length);         // kích thước dữ liệu
                    writer.Write((uint)offset);                   // vị trí dữ liệu
                    offset += images[i].Length;
                }

                for (int i = 0; i < images.Count; i++)
                {
                    writer.Write(images[i]);
                }
            }

            Console.WriteLine("Đã tạo " + output + " (" + widths.Count + " kích thước)");
            return 0;
        }

        /// <summary>Ảnh dạng DIB: BITMAPINFOHEADER + điểm ảnh BGRA (dưới lên) + mặt nạ AND.</summary>
        private static byte[] RenderDib(int size)
        {
            using (Bitmap bitmap = IconDesign.Render(size, true))
            using (MemoryStream buffer = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(buffer))
            {
                int stride = size * 4;
                int maskStride = ((size + 31) / 32) * 4;

                writer.Write(40);              // biSize
                writer.Write(size);            // biWidth
                writer.Write(size * 2);        // biHeight: gộp ảnh và mặt nạ
                writer.Write((ushort)1);       // biPlanes
                writer.Write((ushort)32);      // biBitCount
                writer.Write(0);               // biCompression = BI_RGB
                writer.Write(stride * size);   // biSizeImage
                writer.Write(0);               // biXPelsPerMeter
                writer.Write(0);               // biYPelsPerMeter
                writer.Write(0);               // biClrUsed
                writer.Write(0);               // biClrImportant

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, size, size),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    byte[] row = new byte[stride];
                    for (int y = size - 1; y >= 0; y--)
                    {
                        Marshal.Copy(new IntPtr(data.Scan0.ToInt64() + (long)y * data.Stride), row, 0, stride);
                        writer.Write(row);
                    }
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }

                writer.Write(new byte[maskStride * size]); // mặt nạ AND: độ trong suốt do kênh alpha lo
                writer.Flush();
                return buffer.ToArray();
            }
        }

        private static byte[] RenderPng(int size)
        {
            using (Bitmap bitmap = IconDesign.Render(size, true))
            using (MemoryStream buffer = new MemoryStream())
            {
                bitmap.Save(buffer, ImageFormat.Png);
                return buffer.ToArray();
            }
        }
    }
}
