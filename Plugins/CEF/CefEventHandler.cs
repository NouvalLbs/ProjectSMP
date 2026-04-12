using ProjectSMP;
using ProjectSMP.Entities.Players.Inventory;
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
            var d = root.GetProperty("d");

            switch (e)
            {
                case "uiReady":
                    HandleUiReady(player);
                    break;
                case "inv:open":
                    InventoryCefService.HandleOpen(player);
                    break;
                case "inv:close":
                    InventoryCefService.HandleClose(player);
                    break;
                case "inv:move":
                    InventoryCefService.HandleMove(player,
                        d.GetProperty("fromIndex").GetInt32(),
                        d.GetProperty("toIndex").GetInt32(),
                        d.GetProperty("amount").GetInt32());
                    break;
                case "inv:action":
                    InventoryCefService.HandleAction(player,
                        d.GetProperty("action").GetString() ?? "",
                        d.GetProperty("index").GetInt32(),
                        d.GetProperty("amount").GetInt32());
                    break;
                case "inv:give_request":
                    InventoryCefService.HandleGiveRequest(player,
                        d.GetProperty("index").GetInt32(),
                        d.GetProperty("amount").GetInt32());
                    break;
                case "inv:give_confirm":
                    InventoryCefService.HandleGiveConfirm(player,
                        d.GetProperty("index").GetInt32(),
                        d.GetProperty("amount").GetInt32(),
                        d.GetProperty("targetId").GetInt32());
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