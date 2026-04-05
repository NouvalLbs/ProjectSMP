using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ProjectSMP.Core
{
    public enum Language { EN, ID }

    public static class LocalizationManager
    {
        private const string FilePath = "scriptfiles/Localization.json";
        private const string Fallback = "ID";

        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> _data = new();

        public static void Load()
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException($"Localization file not found: {FilePath}");

            _data = JsonSerializer.Deserialize<
                Dictionary<string, Dictionary<string, Dictionary<string, string>>>
            >(File.ReadAllText(FilePath)) ?? new();
        }

        public static string Get(Language lang, string section, string key)
        {
            var lk = lang.ToString();

            if (_data.TryGetValue(lk, out var s) && s.TryGetValue(section, out var k2) && k2.TryGetValue(key, out var v))
                return v;

            if (_data.TryGetValue(Fallback, out var fs) && fs.TryGetValue(section, out var fk) && fk.TryGetValue(key, out var fv))
                return fv;

            return $"[{section}.{key}]";
        }

        public static string Get(Language lang, string section, string key, params object[] args)
        {
            var raw = Get(lang, section, key);
            for (var i = 0; i < args.Length; i++)
                raw = raw.Replace($"[{i}]", args[i]?.ToString() ?? "");
            return raw;
        }
    }
}