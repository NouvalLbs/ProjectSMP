using ProjectSMP;
using ProjectSMP.Plugins.CEF;
using SampSharp.GameMode.World;
using System;
using System.Text.Json;

public static class CefEventHandler
{
    public static void Initialize()
    {
        CefService.OnClientEvent += Handle;
    }

    private static void Handle(int playerId, string eventName, string payload)
    {
        var player = BasePlayer.Find(playerId) as Player;
        if (player == null) return;

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var e = root.GetProperty("e").GetString() ?? "";
            var d = root.GetProperty("d").GetRawText();

            switch (e)
            {
                case "uiReady":
                    HandleUiReady(player);
                    break;
            }
        }
        catch
        {
            Console.WriteLine($"[CEF] Failed to parse event from player {playerId}");
        }
    }

    private static void HandleUiReady(Player player)
    {
        Console.WriteLine($"[CEF] UI Ready for player {player.Name}");
    }
}