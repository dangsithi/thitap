using System;
using System.Collections.Generic;
using System.Text;

namespace ThiTap.Engine
{
    /// <summary>Kết quả xử lý một phím.</summary>
    internal struct KeyResult
    {
        /// <summary>True khi phím đã bị xử lý và phải chặn không cho ứng dụng nhận.</summary>
        public bool Handled;
        /// <summary>Số ký tự cần xoá trước khi gõ phần thay thế.</summary>
        public int Backspaces;
        /// <summary>Chuỗi cần gõ lại, null nếu không gõ gì.</summary>
        public string Text;

        public static KeyResult PassThrough()
        {
            return new KeyResult();
        }

        public static KeyResult Swallow()
        {
            KeyResult result = new KeyResult();
            result.Handled = true;
            return result;
        }
    }

    /// <summary>
    /// Lõi gõ Telex: nhận chuỗi phím thô của từ đang gõ và trả về chữ tiếng Việt cần hiển thị.
    /// Trạng thái được dựng lại từ toàn bộ phím thô sau mỗi lần gõ nên không cần hoàn tác thủ công.
    /// </summary>
    internal sealed class TelexEngine
    {
        private struct RawKey
        {
            public readonly char Ch;      // chữ thường a-z
            public readonly bool Upper;   // phím được gõ kèm Shift / CapsLock

            public RawKey(char ch, bool upper)
            {
                Ch = ch;
                Upper = upper;
            }
        }

        private struct PlainChar
        {
            public char Ch;
            public bool Upper;

            public PlainChar(char ch, bool upper)
            {
                Ch = ch;
                Upper = upper;
            }
        }

        private readonly List<RawKey> _raw = new List<RawKey>();
        private string _onScreen = string.Empty;

        public bool Enabled { get; set; }

        /// <summary>Cho phép gõ tự do: vẫn đặt dấu dù âm tiết không hợp lệ.</summary>
        public bool AllowFreeTyping { get; set; }

        /// <summary>Bật kiểm tra chính tả: tắt đi thì từ không hợp lệ vẫn được bỏ dấu.</summary>
        public bool SpellCheck { get; set; }

        /// <summary>Tra bảng gõ tắt theo chữ đang hiển thị; null là không dùng gõ tắt.</summary>
        public Func<string, string> MacroLookup { get; set; }

        /// <summary>Vẫn nhớ từ đang gõ khi đang ở chế độ tiếng Anh (để gõ tắt vẫn chạy).</summary>
        public bool MacroWhenDisabled { get; set; }

        /// <summary>Khôi phục đúng phím đã bấm khi từ trông giống tiếng Anh.</summary>
        public bool AutoRestoreEnglish { get; set; }

        /// <summary>Kiểu gõ VNI (dùng 0-9 làm phím dấu) thay vì Telex.</summary>
        public bool Vni { get; set; }

        public TelexEngine()
        {
            Enabled = true;
            SpellCheck = true;
        }

        /// <summary>Chữ đang hiển thị trên màn hình cho từ hiện tại.</summary>
        public string CurrentWord
        {
            get { return _onScreen; }
        }

        /// <summary>Kết thúc từ hiện tại: dấu cách, dấu câu, phím điều hướng, đổi vùng chọn...</summary>
        public void ResetWord()
        {
            if (_raw.Count > 0) _raw.Clear();
            if (_onScreen.Length > 0) _onScreen = string.Empty;
        }

        /// <summary>
        /// Xử lý một phím gõ vào từ: chữ cái a-z ở kiểu Telex, thêm chữ số 0-9 ở kiểu VNI
        /// (upper là trạng thái Shift/CapsLock, luôn false với chữ số).
        /// </summary>
        public KeyResult ProcessLetter(char lower, bool upper)
        {
            if (!Enabled)
            {
                // Đang ở chế độ tiếng Anh: vẫn ghi nhận từ để bảng gõ tắt còn dùng được.
                if (MacroWhenDisabled && MacroLookup != null)
                {
                    _raw.Add(new RawKey(lower, upper));
                    _onScreen = RawLetters();
                }
                else
                {
                    ResetWord();
                }
                return KeyResult.PassThrough();
            }

            _raw.Add(new RawKey(lower, upper));
            string rendered = Render();
            char native = upper ? char.ToUpperInvariant(lower) : lower;

            // Ký tự vừa gõ đã đúng như bàn phím sinh ra: cho phím đi qua, không chèn gì.
            if (rendered.Length == _onScreen.Length + 1
                && rendered[rendered.Length - 1] == native
                && rendered.StartsWith(_onScreen, StringComparison.Ordinal))
            {
                _onScreen = rendered;
                return KeyResult.PassThrough();
            }

            if (string.Equals(rendered, _onScreen, StringComparison.Ordinal))
            {
                return KeyResult.Swallow();
            }

            KeyResult result = new KeyResult();
            result.Handled = true;
            result.Backspaces = _onScreen.Length;
            result.Text = rendered;
            _onScreen = rendered;
            return result;
        }

        /// <summary>Chuỗi phím thô của từ hiện tại, giữ nguyên chữ hoa/thường.</summary>
        public string RawLetters()
        {
            StringBuilder builder = new StringBuilder(_raw.Count);
            for (int i = 0; i < _raw.Count; i++)
            {
                builder.Append(_raw[i].Upper ? char.ToUpperInvariant(_raw[i].Ch) : _raw[i].Ch);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Gọi khi từ vừa kết thúc (dấu cách, dấu câu, Enter...).
        /// Trả về true nếu phải xoá <paramref name="backspaces"/> ký tự và gõ lại <paramref name="replacement"/>.
        /// </summary>
        public bool TryExpand(out string replacement, out int backspaces)
        {
            replacement = null;
            backspaces = 0;
            if (_raw.Count == 0) return false;

            string rendered = _onScreen;
            if (rendered.Length == 0) return false;

            // 1) Gõ tắt: so khớp trên chữ đang hiển thị.
            if (MacroLookup != null)
            {
                string value = MacroLookup(rendered);
                if (!string.IsNullOrEmpty(value) && !string.Equals(value, rendered, StringComparison.Ordinal))
                {
                    replacement = value;
                    backspaces = rendered.Length;
                    return true;
                }
            }

            // 2) Khôi phục phím tiếng Anh: "test" thay vì "tét".
            if (AutoRestoreEnglish && Enabled)
            {
                string raw = RawLetters();
                if (!string.Equals(raw, rendered, StringComparison.Ordinal) && EnglishWords.LooksEnglish(raw))
                {
                    replacement = raw;
                    backspaces = rendered.Length;
                    return true;
                }
            }

            return false;
        }

        private string Render()
        {
            List<PlainChar> plain = new List<PlainChar>();
            bool endsWithConsumedKey;
            int tone = Parse(plain, out endsWithConsumedKey);

            string lower = BuildLower(plain);
            bool valid = Vietnamese.IsValidSyllable(lower);

            // Gõ tự do hoặc tắt kiểm tra chính tả: vẫn hiển thị dấu dù âm tiết không hợp lệ.
            if (valid || AllowFreeTyping || !SpellCheck)
            {
                int toneIndex;
                if (tone == Vietnamese.ToneNone) toneIndex = -1;
                else toneIndex = valid ? Vietnamese.PlaceToneIndex(lower) : Vietnamese.LastVowelIndex(lower);

                StringBuilder buffer = new StringBuilder(lower.Length);
                for (int i = 0; i < lower.Length; i++)
                {
                    char ch = lower[i];
                    if (i == toneIndex) ch = Vietnamese.AddTone(ch, tone);
                    if (plain[i].Upper) ch = char.ToUpperInvariant(ch);
                    buffer.Append(ch);
                }
                return buffer.ToString();
            }

            // Không phải âm tiết tiếng Việt: hiển thị đúng những gì đã gõ.
            // Nếu phím cuối vừa bị dùng để xoá dấu thì nó không còn trên màn hình nữa.
            return endsWithConsumedKey ? RenderPlain(plain) : RenderRaw();
        }

        /// <summary>
        /// Dựng lại từ chuỗi phím thô. Trả về mã thanh (0-5).
        /// <paramref name="endsWithConsumedKey"/> cho biết phím cuối cùng đã bị "nuốt" để xoá dấu.
        /// </summary>
        private int Parse(List<PlainChar> plain, out bool endsWithConsumedKey)
        {
            if (Vni) return ParseVni(plain, out endsWithConsumedKey);

            int tone = Vietnamese.ToneNone;
            char toneKey = '\0';
            int consumedIndex = -1;

            for (int i = 0; i < _raw.Count; i++)
            {
                char key = _raw[i].Ch;
                bool upper = _raw[i].Upper;
                int toneOfKey = ToneOf(key);

                if (toneOfKey != Vietnamese.ToneNone && HasVowel(plain))
                {
                    if (tone != Vietnamese.ToneNone && toneKey == key)
                    {
                        // Gõ lại đúng phím dấu: huỷ dấu và hiện phím đó ra.
                        tone = Vietnamese.ToneNone;
                        toneKey = '\0';
                        plain.Add(new PlainChar(key, upper));
                        consumedIndex = i;
                    }
                    else
                    {
                        tone = toneOfKey;
                        toneKey = key;
                    }
                }
                else if (key == 'z' && plain.Count > 0
                         && (tone != Vietnamese.ToneNone || HasAnyMark(plain)))
                {
                    // z xoá toàn bộ dấu của từ.
                    tone = Vietnamese.ToneNone;
                    toneKey = '\0';
                    for (int j = 0; j < plain.Count; j++)
                    {
                        PlainChar item = plain[j];
                        item.Ch = Vietnamese.RemoveQualityMark(item.Ch);
                        plain[j] = item;
                    }
                    consumedIndex = i;
                }
                else if (key == 'w')
                {
                    if (i > 0 && _raw[i - 1].Ch == 'w')
                    {
                        // Gõ "ww" liền nhau: phím thứ hai huỷ ư do phím đầu tạo ra và hiện đúng chữ w.
                        plain.Add(new PlainChar('w', upper));
                        consumedIndex = i;
                    }
                    else if (i + 1 < _raw.Count && _raw[i + 1].Ch == 'w')
                    {
                        // Phím w đầu của cặp "ww" không hiện ra, nhường chỗ cho w thứ hai.
                        consumedIndex = i;
                    }
                    else
                    {
                        ApplyW(plain, upper);
                    }
                }
                else
                {
                    AppendLetter(plain, key, upper);
                }
            }

            endsWithConsumedKey = consumedIndex == _raw.Count - 1;
            return tone;
        }

        /// <summary>w = ư, hoặc biến u/o/a đứng trước thành ư/ơ/ă; "uo" + w thành "ươ".</summary>
        private static void ApplyW(List<PlainChar> plain, bool upper)
        {
            if (plain.Count == 0)
            {
                plain.Add(new PlainChar('ư', upper));
                return;
            }

            int lastIndex = plain.Count - 1;
            char last = plain[lastIndex].Ch;

            // "uo" + w = "ươ" (nguowif -> người, cuowis -> cưới)
            if (last == 'o' && lastIndex > 0 && plain[lastIndex - 1].Ch == 'u')
            {
                plain[lastIndex - 1] = new PlainChar('ư', plain[lastIndex - 1].Upper);
                plain[lastIndex] = new PlainChar('ơ', plain[lastIndex].Upper);
                return;
            }

            // "uu" + w = "ưu": dấu đặt ở nguyên âm đầu (luuw -> lưu, muuw -> mưu, cuwus -> cứu)
            if (last == 'u' && lastIndex > 0 && plain[lastIndex - 1].Ch == 'u')
            {
                plain[lastIndex - 1] = new PlainChar('ư', plain[lastIndex - 1].Upper);
                return;
            }

            // "ua" + w = "ưa" (chuaw -> chưa, muaw -> mưa, thuaw -> thưa).
            // Riêng sau "q" thì giữ "quă" (quaw -> quă, quawng -> quăng).
            if (last == 'a' && lastIndex > 0 && plain[lastIndex - 1].Ch == 'u'
                && !(lastIndex > 1 && plain[lastIndex - 2].Ch == 'q'))
            {
                plain[lastIndex - 1] = new PlainChar('ư', plain[lastIndex - 1].Upper);
                return;
            }

            char marked = last;
            switch (last)
            {
                case 'u': marked = 'ư'; break;
                case 'o': marked = 'ơ'; break;
                case 'a': marked = 'ă'; break;
            }

            if (marked != last)
            {
                plain[lastIndex] = new PlainChar(marked, plain[lastIndex].Upper);
            }
            else
            {
                plain.Add(new PlainChar('w', upper));
            }
        }

        /// <summary>
        /// Kiểu gõ VNI: 1-5 là thanh điệu, 6 = â/ê/ô, 7 = ư/ơ, 8 = ă, 9 = đ, 0 = bỏ dấu.
        /// Chữ cái gõ đúng như bàn phím (không có luật nhân đôi kiểu aa → â).
        /// </summary>
        private int ParseVni(List<PlainChar> plain, out bool endsWithConsumedKey)
        {
            int tone = Vietnamese.ToneNone;
            char toneKey = '\0';
            int consumedIndex = -1;

            for (int i = 0; i < _raw.Count; i++)
            {
                char key = _raw[i].Ch;
                bool upper = _raw[i].Upper;
                int toneOfKey = VniToneOf(key);
                bool consumed = false;

                if (toneOfKey != Vietnamese.ToneNone)
                {
                    if (!HasVowel(plain))
                    {
                        // Chưa có nguyên âm: đây là chữ số bình thường.
                        plain.Add(new PlainChar(key, false));
                    }
                    else if (tone != Vietnamese.ToneNone && toneKey == key)
                    {
                        // Gõ lại đúng phím dấu: huỷ dấu và hiện phím đó ra.
                        tone = Vietnamese.ToneNone;
                        toneKey = '\0';
                        plain.Add(new PlainChar(key, false));
                    }
                    else
                    {
                        tone = toneOfKey;
                        toneKey = key;
                        consumed = true;
                    }
                }
                else if (key == '0')
                {
                    // 0 xoá toàn bộ dấu của từ.
                    if (plain.Count > 0 && (tone != Vietnamese.ToneNone || HasAnyMark(plain)))
                    {
                        tone = Vietnamese.ToneNone;
                        toneKey = '\0';
                        for (int j = 0; j < plain.Count; j++)
                        {
                            PlainChar item = plain[j];
                            item.Ch = Vietnamese.RemoveQualityMark(item.Ch);
                            plain[j] = item;
                        }
                        consumed = true;
                    }
                    else
                    {
                        plain.Add(new PlainChar('0', false));
                    }
                }
                else if (key >= '6' && key <= '9')
                {
                    // Phím dấu (6-9) không tính là đã "nuốt" như phím thanh điệu,
                    // nên từ không hợp lệ vẫn hiện đủ phím đã gõ (d9 -> d9, d9a -> đa).
                    if (!ApplyVniMark(plain, key)) plain.Add(new PlainChar(key, false));
                }
                else
                {
                    plain.Add(new PlainChar(key, upper));
                }

                if (consumed) consumedIndex = i;
            }

            endsWithConsumedKey = consumedIndex == _raw.Count - 1;
            return tone;
        }

        /// <summary>6 = â/ê/ô, 7 = ư/ơ (uo + 7 = ươ), 8 = ă, 9 = đ. Trả về true nếu phím đã thành dấu.</summary>
        private static bool ApplyVniMark(List<PlainChar> plain, char key)
        {
            if (plain.Count == 0) return false;

            int index = -1;
            char marked = '\0';

            switch (key)
            {
                case '6':
                    index = LastIndexOfAny(plain, "aeo");
                    if (index < 0) return false;
                    marked = plain[index].Ch == 'a' ? 'â' : (plain[index].Ch == 'e' ? 'ê' : 'ô');
                    break;

                case '7':
                    index = LastIndexOfAny(plain, "uo");
                    if (index < 0) return false;
                    if (plain[index].Ch == 'o')
                    {
                        marked = 'ơ';
                        // "uo" + 7 = "ươ" (thuo7ng → thương, nguo7i2 → người)
                        if (index > 0 && plain[index - 1].Ch == 'u')
                        {
                            plain[index - 1] = new PlainChar('ư', plain[index - 1].Upper);
                        }
                    }
                    else
                    {
                        marked = 'ư';
                    }
                    break;

                case '8':
                    index = LastIndexOfAny(plain, "a");
                    if (index < 0) return false;
                    marked = 'ă';
                    break;

                default: // '9' = đ, chỉ đúng khi ngay sau d
                    if (plain[plain.Count - 1].Ch != 'd') return false;
                    index = plain.Count - 1;
                    marked = 'đ';
                    break;
            }

            plain[index] = new PlainChar(marked, plain[index].Upper);
            return true;
        }

        private static int LastIndexOfAny(List<PlainChar> plain, string chars)
        {
            for (int i = plain.Count - 1; i >= 0; i--)
            {
                if (chars.IndexOf(plain[i].Ch) >= 0) return i;
            }
            return -1;
        }

        private static int VniToneOf(char key)
        {
            switch (key)
            {
                case '1': return Vietnamese.ToneAcute;
                case '2': return Vietnamese.ToneGrave;
                case '3': return Vietnamese.ToneHookAbove;
                case '4': return Vietnamese.ToneTilde;
                case '5': return Vietnamese.ToneDotBelow;
                default: return Vietnamese.ToneNone;
            }
        }

        /// <summary>Thêm một chữ cái, xử lý nhân đôi: aa→â, ee→ê, oo→ô, dd→đ.</summary>
        private static void AppendLetter(List<PlainChar> plain, char key, bool upper)
        {
            if (plain.Count > 0 && plain[plain.Count - 1].Ch == key)
            {
                char doubled = key;
                switch (key)
                {
                    case 'a': doubled = 'â'; break;
                    case 'e': doubled = 'ê'; break;
                    case 'o': doubled = 'ô'; break;
                    case 'd': doubled = 'đ'; break;
                }
                if (doubled != key)
                {
                    // Giữ nguyên dấu của key đầu tiên, ví dụ "aA" vẫn cho â.
                    plain[plain.Count - 1] = new PlainChar(doubled, plain[plain.Count - 1].Upper);
                    return;
                }
            }
            plain.Add(new PlainChar(key, upper));
        }

        private static int ToneOf(char key)
        {
            switch (key)
            {
                case 's': return Vietnamese.ToneAcute;
                case 'f': return Vietnamese.ToneGrave;
                case 'r': return Vietnamese.ToneHookAbove;
                case 'x': return Vietnamese.ToneTilde;
                case 'j': return Vietnamese.ToneDotBelow;
                default: return Vietnamese.ToneNone;
            }
        }

        private static bool HasVowel(List<PlainChar> plain)
        {
            for (int i = 0; i < plain.Count; i++)
            {
                if (Vietnamese.IsVowel(plain[i].Ch)) return true;
            }
            return false;
        }

        private static bool HasAnyMark(List<PlainChar> plain)
        {
            for (int i = 0; i < plain.Count; i++)
            {
                if (Vietnamese.HasQualityMark(plain[i].Ch)) return true;
            }
            return false;
        }

        private static string BuildLower(List<PlainChar> plain)
        {
            StringBuilder buffer = new StringBuilder(plain.Count);
            for (int i = 0; i < plain.Count; i++)
            {
                buffer.Append(plain[i].Ch);
            }
            return buffer.ToString();
        }

        private string RenderPlain(List<PlainChar> plain)
        {
            StringBuilder buffer = new StringBuilder(plain.Count);
            for (int i = 0; i < plain.Count; i++)
            {
                char ch = plain[i].Ch;
                buffer.Append(plain[i].Upper ? char.ToUpperInvariant(ch) : ch);
            }
            return buffer.ToString();
        }

        private string RenderRaw()
        {
            StringBuilder buffer = new StringBuilder(_raw.Count);
            for (int i = 0; i < _raw.Count; i++)
            {
                char ch = _raw[i].Ch;
                buffer.Append(_raw[i].Upper ? char.ToUpperInvariant(ch) : ch);
            }
            return buffer.ToString();
        }
    }
}
