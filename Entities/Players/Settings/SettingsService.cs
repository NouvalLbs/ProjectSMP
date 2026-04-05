using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Account;
using ProjectSMP.Entities.Players.Needs;
using ProjectSMP.Entities.Vehicles.Speedo;
using ProjectSMP.Plugins.CEF;
using ProjectSMP.Plugins.RealtimeClock;
using ProjectSMP.Plugins.Streamer;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Settings
{
    internal static class SettingsService
    {
        private static string L(Player p, string s, string k) => LocalizationManager.Get(p.Settings.Language, s, k);
        private static string L(Player p, string s, string k, params object[] a) => LocalizationManager.Get(p.Settings.Language, s, k, a);

        private static readonly string[] EnterExitLabels = {
            "{FF6347}Disable", "{A9A9A9}1 Second(s)", "{B0E0E6}2 Second(s)", "{ADD8E6}3 Second(s)",
            "{87CEEB}4 Second(s)", "{87CEFA}5 Second(s)", "{6495ED}6 Second(s)", "{4682B4}7 Second(s)",
            "{4169E1}8 Second(s)", "{0000FF}9 Second(s)", "{0000CD}10 Second(s)", "{00008B}11 Second(s)",
            "{000080}12 Second(s)", "{191970}13 Second(s)", "{1E90FF}14 Second(s)", "{4682B4}15 Second(s)"
        };

        private static readonly string[] DynamicObjLabels = {
            "{ADD8E6}Very Low", "{FFFF99}Low", "{FFD700}Medium", "{FF6347}High"
        };

        public static void ShowMainSettings(Player player)
        {
            player.ShowTabList(
                "Settings",
                new[] { "Settings", "Status" })
                .WithRows(new[] {
                    new[] { "{ffffff}Discord:", $"{{bcb9b7}}{UserControlService.GetSession(player)?.DiscordId ?? "Not Set"}" },
                    new[] { "{ffffff}Change Password", "" },
                    new[] { "{ffffff}HUD Settings", "" },
                    new[] { "{ffffff}Toggle Settings", "" },
                    new[] { "{ffffff}Enter/Exit Delay:", $"{EnterExitLabels[player.Settings.EnterExit]}" },
                    new[] { "{ffffff}Dynamic Objects Priority:", $"{DynamicObjLabels[player.Settings.DynamicObjectsPriority]}" }
                })
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    switch (e.ListItem)
                    {
                        case 0:
                            player.SendClientMessage(Color.White, $"{Msg.Settings} Discord tidak dapat dirubah.");
                            ShowMainSettings(player);
                            break;
                        case 1:
                            ShowChangePassword(player);
                            break;
                        case 2:
                            ShowHudSettings(player);
                            break;
                        case 3:
                            ShowToggleSettings(player);
                            break;
                        case 4:
                            ShowEnterExitDelay(player);
                            break;
                        case 5:
                            ShowDynamicObjectPriority(player);
                            break;
                    }
                });
        }

        private static void ShowChangePassword(Player player)
        {
            player.ShowInput(
                "Change your password",
                "Sekarang, silakan masukkan password baru yang valid\nSimbol password yang Valid: A-Z, a-z, 0-9, _, [ ], () dan Panjang Minimum password adalah 6 karakter")
                .AsPassword()
                .WithButtons("Change", "Exit")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Password change not implemented yet.");
                });
        }

        public static void ShowHudSettings(Player player)
        {
            var enabled = player.Settings.ShowTime ? "{b2ff47}Enabled" : "{FF6347}Disabled";
            player.ShowTabList(
                "Settings -> HUD Settings",
                new[] { "HUD", "Value" })
                .WithRows(new[] {
                    new[] { "{ffffff}HBE HUD", "" },
                    new[] { "{ffffff}Show Date & Time", $"{enabled}" }
                })
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    switch (e.ListItem)
                    {
                        case 0:
                            ShowHBEHudSettings(player);
                            break;
                        case 1:
                            player.Settings.ShowTime = !player.Settings.ShowTime;
                            ClockTextDrawManager.SetVisible(player.Id, player.Settings.ShowTime);
                            var status = player.Settings.ShowTime ? "{b2ff47}Enabled" : "{FF6347}Disabled";
                            player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Show Time & Date menjadi {status}");
                            break;
                    }
                });
        }

        private static void ShowHBEHudSettings(Player player)
        {

            var modeLabel = player.Settings.HBEMode == 0 ? "CEF Modern" : "TextDraw Modern";
            var rows = new List<string[]>
            {
                new[] { "{ffffff}HBE Style", $"{{bdff66}}{modeLabel}" },
                new[] { "{ffffff}Show Health", GetToggleLabel(player.Settings.ShowHealth) },
                new[] { "{ffffff}Show Armour", GetToggleLabel(player.Settings.ShowArmour) },
                new[] { "{ffffff}Show Hunger", GetToggleLabel(player.Settings.ShowHunger) },
                new[] { "{ffffff}Show Thirst", GetToggleLabel(player.Settings.ShowThirst) },
                new[] { "{ffffff}Show Stress", GetToggleLabel(player.Settings.ShowStress) }
            };

            player.ShowTabList(
                "Settings -> HBE HUD Settings",
                new[] { "HUD", "Value" })
                .WithRows(rows.ToArray())
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    switch (e.ListItem)
                    {
                        case 0:
                            ShowHBEStyleSelection(player);
                            break;
                        case 1:
                            player.Settings.ShowHealth = !player.Settings.ShowHealth;
                            NeedsService.RefreshHud(player);
                            player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Show Health menjadi {GetToggleLabel(player.Settings.ShowHealth)}");
                            break;
                        case 2:
                            player.Settings.ShowArmour = !player.Settings.ShowArmour;
                            NeedsService.RefreshHud(player);
                            player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Show Armour menjadi {GetToggleLabel(player.Settings.ShowArmour)}");
                            break;
                        case 3:
                            player.Settings.ShowHunger = !player.Settings.ShowHunger;
                            NeedsService.RefreshHud(player);
                            player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Show Hunger menjadi {GetToggleLabel(player.Settings.ShowHunger)}");
                            break;
                        case 4:
                            player.Settings.ShowThirst = !player.Settings.ShowThirst;
                            NeedsService.RefreshHud(player);
                            player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Show Thirst menjadi {GetToggleLabel(player.Settings.ShowThirst)}");
                            break;
                        case 5:
                            player.Settings.ShowStress = !player.Settings.ShowStress;
                            NeedsService.RefreshHud(player);
                            player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Show Stress menjadi {GetToggleLabel(player.Settings.ShowStress)}");
                            break;
                    }
                });
        }

        private static void ShowHBEStyleSelection(Player player)
        {
            player.ShowList(
                "Select HBE Style",
                "CEF Modern",
                "TextDraw Modern")
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowHBEHudSettings(player);
                        return;
                    }

                    if (e.ListItem == 0)
                    {
                        if (!CefService.HasPlugin(player.Id))
                        {
                            player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum menginstall CEF Plugin, tidak bisa menggunakan CEF HUD.");
                            ShowHBEHudSettings(player);
                            return;
                        }

                        if (player.Settings.HBEMode != 0)
                        {
                            player.Settings.HBEMode = 0;
                            NeedsHudManager.Cleanup(player);

                            if (player.State == PlayerState.Driving)
                            {
                                SpeedometerService.OnPlayerDisconnect(player);
                                CefService.EmitEvent(player.Id, "setInVehicle", new { value = true });
                            }

                            NeedsService.SendHudDataToCef(player);
                            CefService.EmitEvent(player.Id, "setHudVisible", new { visible = true });
                            player.SendClientMessage(Color.White, $"{Msg.Settings} HBE Mode changed to CEF Modern");
                        }
                        else
                        {
                            ShowHBEHudSettings(player);
                        }
                    }
                    else if (e.ListItem == 1)
                    {
                        if (player.Settings.HBEMode != 1)
                        {
                            player.Settings.HBEMode = 1;
                            CefService.EmitEvent(player.Id, "setHudVisible", new { visible = false });

                            if (player.State == PlayerState.Driving)
                            {
                                CefService.EmitEvent(player.Id, "setInVehicle", new { value = false });
                                SpeedometerService.OnPlayerStateChanged(player, PlayerState.Driving, PlayerState.OnFoot);
                            }

                            NeedsHudManager.Initialize(player);
                            player.SendClientMessage(Color.White, $"{Msg.Settings} HBE Mode changed to TextDraw Modern");
                        }
                        else
                        {
                            ShowHBEHudSettings(player);
                        }
                    }
                });
        }

        public static void ShowToggleSettings(Player player)
        {
            var s = player.Settings;
            var rows = new List<string[]>
            {
                new[] { "{ffffff}Toggle Join Log", GetToggleLabel(s.ToggleJoinLog) },
                new[] { "{ffffff}Toggle News", GetToggleLabel(s.ToggleNews) },
                new[] { "{ffffff}Toggle Quiz", GetToggleLabel(s.ToggleQuiz) },
                new[] { "{ffffff}Toggle Advertise", GetToggleLabel(s.ToggleAdvertise) },
                new[] { "{ffffff}Toggle Uppercase", GetToggleLabel(s.ToggleUppercase) },
                new[] { "{ffffff}Toggle Streamer Mode", GetToggleLabel(s.ToggleStreamerMode) },
                new[] { "{ffffff}Toggle OOC Chat", GetToggleLabel(s.ToggleChatOOC) },
                new[] { "{ffffff}Toggle Family Chat", GetToggleLabel(s.ToggleFamilyChat) },
                new[] { "{ffffff}Toggle Walkie Talkie Chat", GetToggleLabel(s.ToggleWTChat) },
                new[] { "{ffffff}Toggle Faction Radio Chat", GetToggleLabel(s.ToggleFacRadioChat) },
                new[] { "{ffffff}Toggle Private Messages", GetToggleLabel(s.TogglePrivateMessage) },
                new[] { "{ffffff}Toggle Admin Command Log", GetToggleLabel(s.ToggleAdminCmdLog) },
                new[] { "{ffffff}Toggle Auto Low Chat", GetToggleLabel(s.ToggleAutoLowChat) },
                new[] { "{ffffff}Toggle Auto Handbrake", GetToggleLabel(s.ToggleAutoHandbrake) },
                new[] { "{ffffff}Toggle Auto Chat Animation", GetToggleLabel(s.ToggleAutoChatAnimation) },
                new[] { "{ffffff}Toggle Auto Seatbelt/Helmet", GetToggleLabel(s.ToggleSeatbeltHelmet) },
                new[] { "{ffffff}Toggle Auto Mask every Logged In", GetToggleLabel(s.ToggleAutoMaskLoggedIn) }
            };

            player.ShowTabList(
                "Settings -> Toggle Settings",
                new[] { "Toggle", "Value" })
                .WithRows(rows.ToArray())
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    HandleToggleSelection(player, e.ListItem);
                });
        }

        private static void HandleToggleSelection(Player player, int index)
        {
            var s = player.Settings;
            switch (index)
            {
                case 0:
                    s.ToggleJoinLog = !s.ToggleJoinLog;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Join Log menjadi {GetToggleLabel(s.ToggleJoinLog)}");
                    break;
                case 1:
                    s.ToggleNews = !s.ToggleNews;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle News menjadi {GetToggleLabel(s.ToggleNews)}");
                    break;
                case 2:
                    s.ToggleQuiz = !s.ToggleQuiz;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Quiz menjadi {GetToggleLabel(s.ToggleQuiz)}");
                    break;
                case 3:
                    s.ToggleAdvertise = !s.ToggleAdvertise;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Advertise menjadi {GetToggleLabel(s.ToggleAdvertise)}");
                    break;
                case 4:
                    s.ToggleUppercase = !s.ToggleUppercase;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Uppercase menjadi {GetToggleLabel(s.ToggleUppercase)}");
                    break;
                case 5:
                    s.ToggleStreamerMode = !s.ToggleStreamerMode;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Streamermode menjadi {GetToggleLabel(s.ToggleStreamerMode)}");
                    break;
                case 6:
                    s.ToggleChatOOC = !s.ToggleChatOOC;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Chat OOC menjadi {GetToggleLabel(s.ToggleChatOOC)}");
                    break;
                case 7:
                    s.ToggleFamilyChat = !s.ToggleFamilyChat;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Family Chat menjadi {GetToggleLabel(s.ToggleFamilyChat)}");
                    break;
                case 8:
                    s.ToggleWTChat = !s.ToggleWTChat;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Walkie Talkie menjadi {GetToggleLabel(s.ToggleWTChat)}");
                    break;
                case 9:
                    s.ToggleFacRadioChat = !s.ToggleFacRadioChat;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Faction Radio menjadi {GetToggleLabel(s.ToggleFacRadioChat)}");
                    break;
                case 10:
                    s.TogglePrivateMessage = !s.TogglePrivateMessage;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Private Message menjadi {GetToggleLabel(s.TogglePrivateMessage)}");
                    break;
                case 11:
                    s.ToggleAdminCmdLog = !s.ToggleAdminCmdLog;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Admin Command Log menjadi {GetToggleLabel(s.ToggleAdminCmdLog)}");
                    break;
                case 12:
                    s.ToggleAutoLowChat = !s.ToggleAutoLowChat;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Auto Low Chat menjadi {GetToggleLabel(s.ToggleAutoLowChat)}");
                    break;
                case 13:
                    s.ToggleAutoHandbrake = !s.ToggleAutoHandbrake;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Auto Handbrake menjadi {GetToggleLabel(s.ToggleAutoHandbrake)}");
                    break;
                case 14:
                    s.ToggleAutoChatAnimation = !s.ToggleAutoChatAnimation;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Auto Chat Animation menjadi {GetToggleLabel(s.ToggleAutoChatAnimation)}");
                    break;
                case 15:
                    s.ToggleSeatbeltHelmet = !s.ToggleSeatbeltHelmet;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Auto Seatbelt/Helmet menjadi {GetToggleLabel(s.ToggleSeatbeltHelmet)}");
                    break;
                case 16:
                    s.ToggleAutoMaskLoggedIn = !s.ToggleAutoMaskLoggedIn;
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah settings Toggle Auto Mask every Logged In menjadi {GetToggleLabel(s.ToggleAutoMaskLoggedIn)}");
                    break;
            }
        }

        private static void ShowEnterExitDelay(Player player)
        {
            player.ShowList(
                "Delay Enter/Exit",
                "{FF6347}Disable",
                "{B0E0E6}2 Second(s)",
                "{87CEEB}4 Second(s)",
                "{6495ED}6 Second(s)",
                "{0000FF}9 Second(s)",
                "{000080}12 Second(s)",
                "{4682B4}15 Second(s)")
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    var delays = new[] { 0, 2, 4, 6, 9, 12, 15 };
                    player.Settings.EnterExit = delays[e.ListItem];
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu {{bdff66}}berhasil{{FFFFFF}} merubah Delay Enter/Exit ke {EnterExitLabels[player.Settings.EnterExit]}");
                });
        }

        private static void ShowDynamicObjectPriority(Player player)
        {
            player.ShowTabList(
                "Dynamic Object Priority",
                new[] { "Priority", "Rendered Object", "Radius Multiplier" })
                .WithRows(new[] {
                    new[] { "{ADD8E6}Very Low", "150", "0.25" },
                    new[] { "{FFFF99}Low (Default)", "500", "0.5" },
                    new[] { "{FFD700}Medium", "750", "1.0" },
                    new[] { "{FF6347}High", "1000", "2.0" }
                })
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    player.Settings.DynamicObjectsPriority = e.ListItem;
                    ApplyDynamicObjectPriority(player);
                    player.SendClientMessage(Color.White, $"{Msg.Settings} Kamu merubah Dynamic Object Priority ke {DynamicObjLabels[player.Settings.DynamicObjectsPriority]}");
                });
        }

        public static void ApplyDynamicObjectPriority(Player player)
        {
            var settings = new (int visible, float radius)[] {
                (150, 0.25f), (500, 0.5f), (750, 1.0f), (1000, 2.0f)
            };
            var (visible, radius) = settings[player.Settings.DynamicObjectsPriority];

            StreamerNatives.Instance.Streamer_SetVisibleItems(0, visible, player.Id);
            StreamerNatives.Instance.Streamer_SetRadiusMultiplier(0, radius, player.Id);
        }

        private static string GetToggleLabel(bool value) => value ? "{b2ff47}Enabled" : "{FF6347}Disabled";
    }
}