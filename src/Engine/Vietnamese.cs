using System;
using System.Collections.Generic;
using System.Text;

namespace ThiTap.Engine
{
    /// <summary>
    /// Bảng chữ cái, cấu trúc âm tiết và luật đặt dấu thanh của tiếng Việt.
    /// Mọi chuỗi đầu vào ở đây đều là chữ thường, đã dựng sẵn (NFC).
    /// </summary>
    internal static class Vietnamese
    {
        /// <summary>Kiểu cũ: đặt dấu ở oà, uý, khoẻ thay vì oá, úy, khoẻ kiểu mới.</summary>
        public static bool OldStyleTone { get; set; }

        public const int ToneNone = 0;
        public const int ToneAcute = 1;      // sắc
        public const int ToneGrave = 2;      // huyền
        public const int ToneHookAbove = 3;  // hỏi
        public const int ToneTilde = 4;      // ngã
        public const int ToneDotBelow = 5;   // nặng

        // Dấu tổ hợp tương ứng với từng thanh, dùng để dựng ký tự Unicode.
        private static readonly string[] ToneMarks =
        {
            string.Empty, "\u0301", "\u0300", "\u0309", "\u0303", "\u0323"
        };

        // Phụ âm đầu, đặt dài trước ngắn để ưu tiên cách tách có nghĩa ("gi", "qu", "ngh"...).
        // Chuỗi rỗng ở cuối cho phép âm tiết không có phụ âm đầu: a, ơi, ươi...
        private static readonly string[] Onsets =
        {
            "ngh", "ng", "nh", "ch", "gh", "gi", "kh", "ph", "th", "tr", "qu",
            "b", "c", "d", "đ", "g", "h", "k", "l", "m", "n", "p", "r", "s", "t", "v", "x", ""
        };

        private static readonly HashSet<string> CodaSet =
            new HashSet<string>(new[] { "ch", "ng", "nh", "c", "m", "n", "p", "t" }, StringComparer.Ordinal);

        // Các cụm nguyên âm (vần) hợp lệ, không tính phụ âm cuối.
        private static readonly HashSet<string> ClusterSet = new HashSet<string>(new[]
        {
            // một nguyên âm
            "a", "ă", "â", "e", "ê", "i", "o", "ô", "ơ", "u", "ư", "y",
            // hai nguyên âm
            "ai", "ao", "au", "ay", "âu", "ây", "eo", "êu", "ia", "iê", "iu",
            "oa", "oă", "oe", "oi", "ôi", "ơi", "ua", "uâ", "uê", "ui", "uô", "uy", "uơ",
            "ưa", "ưi", "ưu", "ươ", "yê",
            // ba nguyên âm
            "iêu", "oai", "oay", "oeo", "uây", "uôi", "uya", "uyê", "uyu",
            "ươi", "ươu", "yêu"
        }, StringComparer.Ordinal);

        public static bool IsVowel(char c)
        {
            switch (c)
            {
                case 'a': case 'ă': case 'â':
                case 'e': case 'ê':
                case 'i':
                case 'o': case 'ô': case 'ơ':
                case 'u': case 'ư':
                case 'y':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Nguyên âm/phụ âm có dấu phụ chất lượng (ă â ê ô ơ ư) hoặc đ.</summary>
        public static bool HasQualityMark(char c)
        {
            switch (c)
            {
                case 'ă': case 'â': case 'ê': case 'ô': case 'ơ': case 'ư': case 'đ':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Bỏ dấu phụ chất lượng: ă→a, â→a, ê→e, ô→o, ơ→o, ư→u, đ→d.</summary>
        public static char RemoveQualityMark(char c)
        {
            switch (c)
            {
                case 'ă': case 'â': return 'a';
                case 'ê': return 'e';
                case 'ô': case 'ơ': return 'o';
                case 'ư': return 'u';
                case 'đ': return 'd';
                default: return c;
            }
        }

        /// <summary>Thêm dấu thanh vào một nguyên âm, trả về ký tự Unicode dựng sẵn.</summary>
        public static char AddTone(char c, int tone)
        {
            if (tone <= ToneNone || tone > ToneDotBelow) return c;
            string composed = (c.ToString() + ToneMarks[tone]).Normalize(NormalizationForm.FormC);
            return composed[0];
        }

        /// <summary>Kiểm tra chuỗi đã tách được thành phụ âm đầu + vần + phụ âm cuối hợp lệ.</summary>
        public static bool IsValidSyllable(string plain)
        {
            int start, end;
            return TryFindCluster(plain, out start, out end);
        }

        /// <summary>
        /// Vị trí đặt dấu thanh trong cả từ (chỉ số ký tự), hoặc -1 nếu không tách được âm tiết.
        /// </summary>
        public static int PlaceToneIndex(string plain)
        {
            int start, end;
            if (!TryFindCluster(plain, out start, out end)) return -1;

            string cluster = plain.Substring(start, end - start + 1);
            bool hasCoda = end < plain.Length - 1;
            return start + TonePositionInCluster(cluster, hasCoda);
        }

        /// <summary>
        /// Luật đặt dấu: ưu tiên nguyên âm đã có dấu phụ, sau đó theo số lượng nguyên âm.
        /// Với oa/oe/uy không có phụ âm cuối: kiểu mới đặt ở nguyên âm đầu (hòa, thủy),
        /// kiểu cũ đặt ở nguyên âm sau (hoà, thuỷ) — bật bằng <see cref="OldStyleTone"/>.
        /// </summary>
        private static int TonePositionInCluster(string cluster, bool hasCoda)
        {
            int index = cluster.IndexOf('ơ');
            if (index >= 0) return index;

            index = IndexOfAny(cluster, 'ê', 'â', 'ô');
            if (index >= 0) return index;

            index = cluster.IndexOf('ă');
            if (index >= 0) return index;

            index = cluster.IndexOf('ư');
            if (index >= 0) return index;

            if (cluster.Length >= 3) return 1;
            if (cluster.Length == 2)
            {
                if (!hasCoda && OldStyleTone && IsGlideCluster(cluster)) return 1;
                return hasCoda ? 1 : 0;
            }
            return 0;
        }

        private static bool IsGlideCluster(string cluster)
        {
            return string.Equals(cluster, "oa", StringComparison.Ordinal)
                || string.Equals(cluster, "oe", StringComparison.Ordinal)
                || string.Equals(cluster, "uy", StringComparison.Ordinal);
        }

        /// <summary>Vị trí nguyên âm cuối cùng trong từ, dùng khi gõ tự do (không kiểm tra âm tiết).</summary>
        public static int LastVowelIndex(string plain)
        {
            for (int i = plain.Length - 1; i >= 0; i--)
            {
                if (IsVowel(plain[i])) return i;
            }
            return -1;
        }

        private static int IndexOfAny(string text, char a, char b, char c)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch == a || ch == b || ch == c) return i;
            }
            return -1;
        }

        /// <summary>Tách từ thành phụ âm đầu, vần và phụ âm cuối; trả về false nếu không hợp lệ.</summary>
        private static bool TryFindCluster(string plain, out int start, out int end)
        {
            start = -1;
            end = -1;
            if (string.IsNullOrEmpty(plain)) return false;

            for (int i = 0; i < Onsets.Length; i++)
            {
                string onset = Onsets[i];
                if (plain.Length <= onset.Length) continue;
                if (string.CompareOrdinal(plain, 0, onset, 0, onset.Length) != 0) continue;

                int from = onset.Length;
                int last = LastVowelIndex(plain, from);
                if (last < 0) continue;

                string cluster = plain.Substring(from, last - from + 1);
                if (!ClusterSet.Contains(cluster)) continue;

                string coda = plain.Substring(last + 1);
                if (coda.Length > 0 && !CodaSet.Contains(coda)) continue;

                start = from;
                end = last;
                return true;
            }
            return false;
        }

        private static int LastVowelIndex(string plain, int from)
        {
            for (int i = plain.Length - 1; i >= from; i--)
            {
                if (IsVowel(plain[i])) return i;
            }
            return -1;
        }
    }
}
