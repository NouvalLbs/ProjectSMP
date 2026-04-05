using System.Collections.Generic;

namespace ProjectSMP.Extensions
{
    public static class PlayerDataExtensions
    {
        private static readonly Dictionary<int, Dictionary<string, object>> _playerData = new();

        public static T GetData<T>(this Player player, string key, T defaultValue = default)
        {
            if (!_playerData.TryGetValue(player.Id, out var dict))
                return defaultValue;

            if (!dict.TryGetValue(key, out var value))
                return defaultValue;

            return value is T typedValue ? typedValue : defaultValue;
        }

        public static void SetData<T>(this Player player, string key, T value)
        {
            if (!_playerData.ContainsKey(player.Id))
                _playerData[player.Id] = new Dictionary<string, object>();

            _playerData[player.Id][key] = value;
        }

        public static void ClearPlayerData(this Player player)
        {
            _playerData.Remove(player.Id);
        }
    }
}