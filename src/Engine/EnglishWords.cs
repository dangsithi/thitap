using System;
using System.Collections.Generic;

namespace ThiTap
{
    /// <summary>
    /// Nhận biết từ tiếng Anh thường gặp để khôi phục lại đúng phím đã bấm
    /// (ví dụ "test" bị Telex biến thành "tét" thì trả về "test").
    /// </summary>
    internal static class EnglishWords
    {
        private static readonly HashSet<string> Words = new HashSet<string>(StringComparer.Ordinal)
        {
            "a", "about", "above", "access", "across", "add", "address", "after", "again", "all",
            "also", "always", "am", "analysis", "and", "answer", "any", "app", "are", "array",
            "as", "ask", "at", "back", "bad", "base", "be", "because", "been", "before", "best",
            "better", "between", "big", "bit", "body", "bool", "boss", "both", "box", "break",
            "bring", "build", "business", "but", "buy", "by", "call", "can", "case", "cell",
            "change", "check", "cheese", "child", "class", "clean", "clear", "click", "close",
            "code", "coffee", "cold", "come", "comment", "company", "config", "copy", "cost",
            "could", "count", "country", "create", "cross", "current", "cut", "data", "day",
            "deep", "delete", "design", "detail", "did", "die", "do", "does", "done", "down",
            "each", "early", "easy", "edit", "else", "end", "enough", "enter", "error", "even",
            "event", "ever", "every", "example", "exit", "expect", "fail", "false", "fast",
            "few", "field", "file", "find", "fine", "first", "fix", "focus", "follow", "for",
            "form", "found", "free", "fresh", "from", "front", "full", "function", "game",
            "get", "give", "go", "good", "got", "great", "green", "group", "grow", "guess",
            "had", "half", "hand", "happen", "happy", "has", "have", "he", "head", "hello",
            "help", "her", "here", "high", "him", "his", "hold", "home", "hope", "hot", "hour",
            "how", "however", "huge", "i", "idea", "if", "image", "import", "in", "index",
            "info", "inside", "instead", "int", "into", "is", "issue", "it", "item", "its",
            "job", "join", "just", "keep", "key", "kind", "know", "last", "late", "later",
            "learn", "least", "leave", "left", "less", "let", "level", "life", "light", "like",
            "line", "link", "list", "little", "live", "load", "local", "lock", "log", "long",
            "look", "loop", "lose", "lot", "love", "low", "made", "main", "make", "man", "many",
            "map", "mark", "may", "me", "mean", "meet", "menu", "message", "method", "middle",
            "might", "mind", "minute", "miss", "mode", "model", "more", "most", "move", "much",
            "must", "my", "name", "need", "never", "new", "next", "nice", "no", "node", "none",
            "normal", "not", "note", "nothing", "now", "null", "number", "of", "off", "offer",
            "office", "often", "ok", "old", "on", "once", "one", "only", "open", "option",
            "or", "order", "other", "our", "out", "over", "own", "page", "paper", "part",
            "pass", "password", "path", "people", "per", "person", "pick", "place", "plan",
            "play", "please", "plus", "point", "post", "power", "press", "price", "print",
            "problem", "process", "product", "project", "public", "pull", "push", "put",
            "question", "quick", "quite", "range", "rate", "read", "ready", "real", "reason",
            "red", "release", "remove", "reset", "result", "return", "right", "run", "said",
            "same", "save", "say", "school", "screen", "script", "search", "second", "see",
            "seem", "select", "self", "send", "server", "service", "set", "setup", "she",
            "short", "should", "show", "side", "simple", "since", "size", "so", "some",
            "something", "sort", "sound", "source", "space", "speed", "start", "state", "stay",
            "still", "stop", "store", "string", "struct", "study", "style", "such", "support",
            "sure", "system", "table", "take", "talk", "task", "team", "tell", "test", "tests",
            "text", "than", "thank", "that", "the", "their", "them", "then", "there", "these",
            "they", "thing", "think", "this", "those", "though", "three", "through", "time",
            "to", "today", "together", "too", "tool", "top", "total", "tree", "true", "try",
            "turn", "two", "type", "under", "unit", "until", "up", "update", "us", "use",
            "user", "users", "using", "value", "very", "view", "want", "was", "way", "we",
            "week", "well", "were", "what", "when", "where", "which", "while", "white", "who",
            "why", "will", "window", "with", "without", "word", "work", "world", "would",
            "write", "yes", "yet", "you", "your", "zero"
        };

        private static readonly string[] Suffixes = { "s", "es", "ed", "ing", "ly", "er", "ers", "est" };

        /// <summary>Chuỗi phím gõ ra có phải là một từ tiếng Anh quen thuộc không.</summary>
        public static bool LooksEnglish(string raw)
        {
            if (raw == null) return false;
            string lower = raw.ToLowerInvariant();
            if (lower.Length < 2) return false;
            if (Words.Contains(lower)) return true;

            for (int i = 0; i < Suffixes.Length; i++)
            {
                string suffix = Suffixes[i];
                if (lower.Length <= suffix.Length + 1) continue;
                if (!lower.EndsWith(suffix, StringComparison.Ordinal)) continue;
                if (Words.Contains(lower.Substring(0, lower.Length - suffix.Length))) return true;
            }
            return false;
        }
    }
}
