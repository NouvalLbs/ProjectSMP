using System;
using System.Collections.Generic;
using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Core
{
    public enum ClientType
    {
        PC,
        Android
    }

    public static class ClientManager
    {
        private static readonly HashSet<string> MobileCISerials = new(StringComparer.OrdinalIgnoreCase) {
            "ED40ED0E8089CC44C08EE9580F4C8C44EE8EE990"
        };

        public static event Action<Player> OnMobilePlayerConnected;

        public static void CheckPlayerClient(Player player)
        {
            try
            {
                var ci = player.GPCI?.Trim() ?? string.Empty;
                var version = player.Version?.Trim() ?? string.Empty;

                player.ClientType = ResolveClientType(ci, version);
                player.ClientVersion = version;
                player.ClientCISerial = ci;

                if (player.ClientType == ClientType.Android)
                    OnMobilePlayerConnected?.Invoke(player);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClientManager] Error on {player.Name}: {ex.Message}");
                player.ClientType = ClientType.PC;
                player.ClientVersion = string.Empty;
                player.ClientCISerial = string.Empty;
            }
        }

        public static ClientType ResolveClientType(string ci, string version)
        {
            if (!string.IsNullOrEmpty(ci) && MobileCISerials.Contains(ci))
                return ClientType.Android;

            return ClientType.PC;
        }

        public static bool IsAndroid(Player player) => player.ClientType == ClientType.Android;
        public static bool IsPC(Player player) => player.ClientType == ClientType.PC;

        public static string GetClientName(ClientType type) => type switch
        {
            ClientType.Android => "Android",
            _ => "PC"
        };

        public static Color GetClientColor(ClientType type) => type switch
        {
            ClientType.Android => new Color(152, 251, 152),
            _ => new Color(100, 149, 237)
        };
    }
}