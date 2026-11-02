using System;
using System.Collections.Generic;
using System.Text;
using ThiTap.Engine;

namespace ThiTap.Test
{
    /// <summary>Kiểm thử lõi Telex mà không cần bàn phím thật: gõ từng phím rồi so sánh chữ cuối cùng.</summary>
    internal static class TelexTest
    {
        private static int _failed;

        private static int Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            Check("tieengs", "tiếng");
            Check("tiengs", "tiengs");
            Check("vieetj", "việt");
            Check("VIEETJ", "VIỆT");
            Check("Tieengs", "Tiếng");
            Check("dd", "dd");
            Check("dda", "đa");
            Check("ddaif", "đài");
            Check("dduowngf", "đường");
            Check("dduowcj", "được");
            Check("nguowif", "người");
            Check("cuowis", "cưới");

            // "uu" + w = "ưu": dấu nằm ở nguyên âm đầu, không phải "luư"
            Check("luuw", "lưu");
            Check("luuwj", "lựu");
            Check("muuw", "mưu");
            Check("cuwus", "cứu");
            Check("huuw", "hưu");

            // "ua" + w = "ưa", trừ sau "q" thì vẫn là "quă"
            Check("chuaw", "chưa");
            Check("chuwa", "chưa");
            Check("chuawr", "chửa");
            Check("muaw", "mưa");
            Check("thuaw", "thưa");
            Check("cuaw", "cưa");
            Check("quaw", "quă");
            Check("quawng", "quăng");
            Check("phair", "phải");
            Check("phari", "phải");
            Check("tooi", "tôi");
            Check("toi", "toi");
            Check("chaof", "chào");
            Check("mua", "mua");
            Check("muas", "múa");
            Check("cuar", "của");
            Check("hoa", "hoa");
            Check("hoaf", "hòa");
            Check("hoanf", "hoàn");
            Check("xoair", "xoải");
            Check("xoaif", "xoài");
            Check("quar", "quả");
            Check("quyeenr", "quyển");
            Check("quyeets", "quyết");
            Check("nghieengs", "nghiếng");
            Check("nghieemj", "nghiệm");
            Check("thuowng", "thương");
            Check("uowngs", "ướng");
            Check("truowngf", "trường");
            Check("huowngr", "hưởng");
            Check("muowif", "mười");
            Check("khoer", "khỏe");
            Check("hoir", "hỏi");
            Check("phowr", "phở");
            Check("mowis", "mới");
            Check("muoons", "muốn");
            Check("cuoocj", "cuộc");
            Check("hieeur", "hiểu");
            Check("yeeu", "yêu");
            Check("gif", "gì");
            Check("giaf", "già");
            Check("gia", "gia");
            Check("khuyur", "khuỷu");
            Check("as", "á");
            Check("ass", "as");
            Check("az", "az");
            Check("asz", "a");
            Check("aa", "â");
            Check("aaa", "aaa");
            Check("w", "ư");
            Check("uw", "ư");
            Check("ow", "ơ");
            Check("aw", "ă");

            // Gõ w hai lần liền nhau: phím thứ hai huỷ ư và hiện ra đúng chữ w.
            Check("ww", "w");
            Check("WW", "W");
            Check("www", "ww");
            Check("uww", "uw");
            Check("test", "tét");
            Check("hello", "hello");
            Check("asd", "asd");
            Check("xin", "xin");

            // Xoá lui rồi gõ tiếp: phím dấu phải đặt lại lên chữ còn lại,
            // không được để ký tự thô rơi ra ngoài ("hô" + "ji" thành "hôji").
            BackspaceCheck("hooij", 1, "", "hộ");
            BackspaceCheck("hooij", 1, "j", "hộ");
            BackspaceCheck("hooij", 1, "ij", "hội");
            BackspaceCheck("hooij", 2, "i", "hi");
            BackspaceCheck("as", 1, "as", "á");
            BackspaceCheck("dd", 1, "d", "dd");
            BackspaceCheck("dd", 2, "d", "d");
            BackspaceCheck("dda", 1, "a", "đa");
            BackspaceCheck("vieetj", 1, "tj", "việt");
            BackspaceCheck("xin", 1, "n", "xin");
            BackspaceCheck("tieengs", 1, "gs", "tiếng");
            BackspaceCheck("nguowif", 1, "", null);
            BackspaceCheck("dduowcj", 1, "", null);
            BackspaceCheckVni("hom2", 1, "m2", "hòm");
            BackspaceCheckVni("bua1", 1, "a1", "búa");
            BackspaceCheckVni("vie6t5", 1, "t5", "việt");

            // Đoạn văn đầy đủ dấu (không kèm dấu câu: dấu câu do tầng bàn phím xử lý).
            Sentence(
                "Tooi ddang vieets ddoanj vawn nayf ddeer kieemr tra booj gox tieengs Vieetj",
                "Tôi đang viết đoạn văn này để kiểm tra bộ gõ tiếng Việt");
            Sentence(
                "Chuwowng trinhf dduwowcj luuw owr thuw mucj bin cungf vowis file caaus hinhf",
                "Chương trình được lưu ở thư mục bin cùng với file cấu hình");
            Sentence(
                "Nguowif dungf cos theer baatj tawts chees ddooj tuwj ddoongj",
                "Người dùng có thể bật tắt chế độ tự động");
            Sentence(
                "Car hai cais ddeeuf hoatj ddoongj toots neeus caaus hinhf ddungs",
                "Cả hai cái đều hoạt động tốt nếu cấu hình đúng");

            // Gõ tắt: chốt từ bằng dấu cách thì mở rộng theo bảng.
            Expand("vn", "Việt Nam", true);
            Expand("ko", "không", true);
            Expand("abc", null, true);
            Expand("vn", null, false);

            // Khôi phục phím tiếng Anh.
            Restore("test", "test");
            Restore("his", "his");
            Restore("is", "is");
            Restore("tieengs", null);
            Restore("ddang", null);

            // Kiểu gõ VNI: 1-5 thanh điệu, 6 = â/ê/ô, 7 = ư/ơ, 8 = ă, 9 = đ, 0 = bỏ dấu.
            Vni("tie6ng1", "tiếng");
            Vni("vie6t5", "việt");
            Vni("Vie6t5", "Việt");
            Vni("d9u7o7ng2", "đường");
            Vni("nguo7i2", "người");
            Vni("thuo7ng", "thương");
            Vni("a8n", "ăn");
            Vni("ca8n", "căn");
            Vni("d9a2", "đà");
            Vni("d9a4", "đã");
            Vni("cuo6i1", "cuối");
            Vni("muo6n1", "muốn");
            Vni("cha2o", "chào");
            Vni("hoa3", "hỏa");
            Vni("a1", "á");
            Vni("tie6ng1z", "tie6ng1z");
            Vni("a66", "a66");
            Vni("abc123", "abc");
            Vni("123", "123");
            Vni("d9", "d9");
            Vni("tie6ng0", "tieng");
            Vni("tuo6i3", "tuổi");
            Vni("nghie6ng", "nghiêng");
            Vni("quye6n2", "quyền");
            Vni("khuy3u", "khuỷu");
            Vni("giu7a4", "giữa");
            Vni("d9e6m1", "đếm");
            Vni("a8m", "ăm");
            Vni("cha8n5", "chặn");
            Vni("u7u", "ưu");

            // Cả câu theo kiểu VNI.
            SentenceVni("Toi6 d9ang vie6t1 tie6ng1 Vie6t5", "Tôi đang viết tiếng Việt");
            SentenceVni("Chu7o7ng trinh2 d9u7o7c5 lu7u o73 thu7 muc5 bin",
                "Chương trình được lưu ở thư mục bin");

            Console.WriteLine();
            Console.WriteLine(_failed == 0
                ? "TẤT CẢ ĐỀU ĐÚNG"
                : _failed + " TRƯỜNG HỢP SAI");
            return _failed == 0 ? 0 : 1;
        }

        /// <summary>Kiểm một câu: chuỗi phím của từng từ cách nhau bằng dấu cách.</summary>
        private static void Sentence(string keys, string expected)
        {
            string[] words = keys.Split(' ');
            string[] wanted = expected.Split(' ');

            if (words.Length != wanted.Length)
            {
                _failed++;
                Console.WriteLine("SAI: câu \"" + keys + "\" có " + words.Length
                    + " từ, mong đợi " + wanted.Length);
                return;
            }

            List<string> actual = new List<string>();
            for (int i = 0; i < words.Length; i++)
            {
                TelexEngine engine = new TelexEngine();
                for (int k = 0; k < words[i].Length; k++)
                {
                    char key = char.ToLowerInvariant(words[i][k]);
                    engine.ProcessLetter(key, char.IsUpper(words[i][k]));
                }
                actual.Add(engine.CurrentWord);
            }

            string got = string.Join(" ", actual.ToArray());
            if (string.Equals(got, expected, StringComparison.Ordinal)) return;

            _failed++;
            Console.WriteLine("SAI: câu gõ \"" + keys + "\"");
            Console.WriteLine("   mong đợi: " + expected + "  " + Codes(expected));
            Console.WriteLine("   thực tế : " + got + "  " + Codes(got));
        }

        private static readonly Dictionary<string, string> Macros = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "vn", "Việt Nam" },
            { "ko", "không" }
        };

        private static string LookupMacro(string word)
        {
            string value;
            return Macros.TryGetValue(word, out value) ? value : null;
        }

        /// <summary>Gõ tắt: gõ khoá, chốt từ, xem nội dung thay thế.</summary>
        private static void Expand(string keys, string expected, bool enabled)
        {
            TelexEngine engine = new TelexEngine();
            engine.MacroLookup = enabled ? new Func<string, string>(LookupMacro) : null;
            for (int i = 0; i < keys.Length; i++)
            {
                engine.ProcessLetter(keys[i], false);
            }

            string replacement;
            int backspaces;
            bool expanded = engine.TryExpand(out replacement, out backspaces);

            if (expected == null)
            {
                if (expanded)
                {
                    _failed++;
                    Console.WriteLine("SAI: \"" + keys + "\" không có trong bảng nhưng vẫn mở rộng");
                }
                return;
            }

            if (expanded && replacement == expected && backspaces == engine.CurrentWord.Length) return;

            _failed++;
            Console.WriteLine("SAI gõ tắt: \"" + keys + "\" -> \"" + (replacement == null ? "(không)" : replacement)
                + "\", mong đợi \"" + expected + "\"");
        }

        /// <summary>Khôi phục phím tiếng Anh: chốt từ thì trả về đúng ký tự đã bấm.</summary>
        private static void Restore(string keys, string expected)
        {
            TelexEngine engine = new TelexEngine();
            engine.AutoRestoreEnglish = true;
            for (int i = 0; i < keys.Length; i++)
            {
                engine.ProcessLetter(keys[i], false);
            }

            string replacement;
            int backspaces;
            bool restored = engine.TryExpand(out replacement, out backspaces);

            if (expected == null)
            {
                if (restored)
                {
                    _failed++;
                    Console.WriteLine("SAI: \"" + keys + "\" không nên khôi phục nhưng vẫn khôi phục");
                }
                return;
            }

            if (restored && replacement == expected) return;

            _failed++;
            Console.WriteLine("SAI khôi phục: \"" + keys + "\" -> \"" + (replacement == null ? "(không)" : replacement)
                + "\", mong đợi \"" + expected + "\"");
        }

        /// <summary>Gõ theo kiểu VNI rồi so sánh chữ cuối cùng.</summary>
        private static void Vni(string keys, string expected)
        {
            TelexEngine engine = new TelexEngine();
            engine.Vni = true;
            for (int i = 0; i < keys.Length; i++)
            {
                char key = char.ToLowerInvariant(keys[i]);
                engine.ProcessLetter(key, char.IsUpper(keys[i]));
            }

            if (string.Equals(engine.CurrentWord, expected, StringComparison.Ordinal)) return;

            _failed++;
            Console.WriteLine("SAI VNI: \"" + keys + "\" -> " + Codes(engine.CurrentWord)
                + ", mong đợi " + Codes(expected));
        }

        /// <summary>Kiểm một câu gõ theo kiểu VNI.</summary>
        private static void SentenceVni(string keys, string expected)
        {
            string[] words = keys.Split(' ');
            string[] wanted = expected.Split(' ');

            if (words.Length != wanted.Length)
            {
                _failed++;
                Console.WriteLine("SAI: câu VNI \"" + keys + "\" có " + words.Length
                    + " từ, mong đợi " + wanted.Length);
                return;
            }

            List<string> actual = new List<string>();
            for (int i = 0; i < words.Length; i++)
            {
                TelexEngine engine = new TelexEngine();
                engine.Vni = true;
                for (int k = 0; k < words[i].Length; k++)
                {
                    char key = char.ToLowerInvariant(words[i][k]);
                    engine.ProcessLetter(key, char.IsUpper(words[i][k]));
                }
                actual.Add(engine.CurrentWord);
            }

            string got = string.Join(" ", actual.ToArray());
            if (string.Equals(got, expected, StringComparison.Ordinal)) return;

            _failed++;
            Console.WriteLine("SAI: câu VNI \"" + keys + "\"");
            Console.WriteLine("   mong đợi: " + expected + "  " + Codes(expected));
            Console.WriteLine("   thực tế : " + got + "  " + Codes(got));
        }

        private static void Check(string keys, string expected)
        {
            TelexEngine engine = new TelexEngine();
            for (int i = 0; i < keys.Length; i++)
            {
                char key = char.ToLowerInvariant(keys[i]);
                engine.ProcessLetter(key, char.IsUpper(keys[i]));
            }

            string actual = engine.CurrentWord;
            if (actual == expected) return;

            _failed++;
            Console.WriteLine("SAI: gõ \"" + keys + "\"");
            Console.WriteLine("   mong đợi: " + expected + "  " + Codes(expected));
            Console.WriteLine("   thực tế : " + actual + "  " + Codes(actual));
        }

        /// <summary>Gõ một chuỗi phím và mô phỏng lại đúng những gì ứng dụng hiển thị.</summary>
        private static string ApplyKeys(TelexEngine engine, string screen, string keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                char key = char.ToLowerInvariant(keys[i]);
                bool upper = char.IsUpper(keys[i]);
                KeyResult result = engine.ProcessLetter(key, upper);

                if (!result.Handled)
                {
                    // Phím đi nguyên vẹn qua ứng dụng.
                    screen += upper ? char.ToUpperInvariant(key) : key;
                    continue;
                }

                if (result.Backspaces > screen.Length)
                {
                    _failed++;
                    Console.WriteLine("SAI: lõi gõ xin xoá " + result.Backspaces
                        + " chữ nhưng màn hình chỉ có \"" + screen + "\"");
                    return screen;
                }

                screen = screen.Substring(0, screen.Length - result.Backspaces);
                if (!string.IsNullOrEmpty(result.Text)) screen += result.Text;
            }

            return screen;
        }

        /// <summary>
        /// Kiểm chuỗi gõ có bấm Backspace giữa chừng: gõ <paramref name="keys"/>, xoá
        /// <paramref name="deletes"/> lần, gõ tiếp <paramref name="more"/> rồi so chữ trên màn hình.
        /// expected = null nghĩa là chỉ yêu cầu lõi gõ còn đúng số chữ đang hiển thị.
        /// </summary>
        private static void BackspaceCheck(string keys, int deletes, string more, string expected)
        {
            RunBackspace(new TelexEngine(), keys, deletes, more, expected);
        }

        /// <summary>Như BackspaceCheck nhưng gõ theo kiểu VNI.</summary>
        private static void BackspaceCheckVni(string keys, int deletes, string more, string expected)
        {
            TelexEngine engine = new TelexEngine();
            engine.Vni = true;
            RunBackspace(engine, keys, deletes, more, expected);
        }

        private static void RunBackspace(TelexEngine engine, string keys, int deletes, string more, string expected)
        {
            string screen = ApplyKeys(engine, string.Empty, keys);

            for (int i = 0; i < deletes; i++)
            {
                engine.ProcessBackspace();
                if (screen.Length > 0) screen = screen.Substring(0, screen.Length - 1);

                if (engine.CurrentWord.Length != screen.Length)
                {
                    _failed++;
                    Console.WriteLine("SAI Backspace: \"" + keys + "\" xoá " + (i + 1) + " lần");
                    Console.WriteLine("   màn hình: \"" + screen + "\" (" + screen.Length + " chữ)  " + Codes(screen));
                    Console.WriteLine("   lõi gõ  : \"" + engine.CurrentWord + "\" (" + engine.CurrentWord.Length + " chữ)  " + Codes(engine.CurrentWord));
                    return;
                }
            }

            screen = ApplyKeys(engine, screen, more);
            if (expected == null) return;

            if (string.Equals(screen, expected, StringComparison.Ordinal)) return;

            _failed++;
            Console.WriteLine("SAI Backspace: \"" + keys + "\" xoá " + deletes + " lần rồi gõ \"" + more + "\"");
            Console.WriteLine("   mong đợi: " + expected + "  " + Codes(expected));
            Console.WriteLine("   thực tế : " + screen + "  " + Codes(screen));
        }

        private static string Codes(string text)
        {
            List<string> parts = new List<string>();
            for (int i = 0; i < text.Length; i++)
            {
                parts.Add("U+" + ((int)text[i]).ToString("X4"));
            }
            return "[" + string.Join(" ", parts.ToArray()) + "]";
        }
    }
}
