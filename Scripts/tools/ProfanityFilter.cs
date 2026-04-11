using UnityEngine;

namespace tools
{
    public static class ProfanityFilter
    {
        private static string[] _words;

        public static bool ContainsProfanity(string text)
        {
            if (_words == null) Load();
            var lower = text.ToLower();
            foreach (var word in _words)
                if (!string.IsNullOrEmpty(word) && lower.Contains(word))
                    return true;
            return false;
        }

        private static void Load()
        {
            var asset = Resources.Load<TextAsset>("Data/SwearWords");
            _words = asset.text.Split(',');
        }
    }
}
