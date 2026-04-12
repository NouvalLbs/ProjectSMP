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

        Console.WriteLine($"[CEF] Handle: player={playerId} event={eventName} payload={payload}");

        try
        {
            using var doc = JsonDocument.Parse(payload.Length > 0 ? payload : "{}");
            var d = doc.RootElement;

            switch (eventName)
            {
                case "uiReady":
                    HandleUiReady(player);
                    break;

                case "inv:open":
                    InventoryCefService.HandleOpen(player);
                    CefService.FocusBrowser(player.Id, 1, true);
                    break;

                case "inv:close":
                    InventoryCefService.HandleClose(player);
                    CefService.FocusBrowser(player.Id, 1, false);
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

                case "inv:drop_to_inv":
                    InventoryCefService.HandleDropToInv(player,
                        d.GetProperty("dropSlot").GetInt32(),
                        d.GetProperty("toSlot").GetInt32(),
                        d.GetProperty("amount").GetInt32());
                    break;

                case "inv:inv_to_drop":
                    InventoryCefService.HandleInvToDrop(player,
                        d.GetProperty("fromSlot").GetInt32(),
                        d.GetProperty("amount").GetInt32());
                    break;

                case "inv:drop_take":
                    InventoryCefService.HandleDropTake(player,
                        d.GetProperty("slot").GetInt32(),
                        d.GetProperty("amount").GetInt32());
                    break;

                default:
                    Console.WriteLine($"[CEF] Unknown event: {eventName}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CEF] Error handling '{eventName}': {ex.Message}");
        }
    }

    private static void HandleUiReady(Player player)
    {
        Console.WriteLine($"[CEF] UI Ready: {player.Name}");
    }
}