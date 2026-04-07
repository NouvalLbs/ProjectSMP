#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator;
using ProjectSMP.Entities.Players.Condition;
using ProjectSMP.Entities.Players.Delay.Data;
using ProjectSMP.Entities.Players.Inventory.Data;
using ProjectSMP.Entities.Players.NameTag;
using ProjectSMP.Entities.Players.Needs;
using ProjectSMP.Entities.Players.Settings;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Bank.Paycheck;
using ProjectSMP.Features.CinematicCamera;
using ProjectSMP.Features.EnterExit;
using ProjectSMP.Features.LevelSystem;
using ProjectSMP.Features.PreviewModelDialog;
using ProjectSMP.Plugins.RealtimeClock;
using ProjectSMP.Plugins.Streamer;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectSMP.Entities.Players.Character
{
    public class CharPosition { public float X, Y, Z, A; public int Interior, World; }
    public class CharVitals { public float MaxHealth = 100, Health = 100, Armour, Hunger = 100, Energy = 100, Stress = 0; }
    public class CharPlaytime { public int Hours, Minutes, Seconds; }

    public class CharPhone { public int Number, Off, Credit; }
    public class CharJailInfo { public int Jailed, Time; public string Reason = ""; }
    public class CharBanInfo { public int Banned, Time, Expire; public string Reason = "", Admin = ""; }

    public class CharCondition
    {
        public int DyingTime, Injured;
        public int DyingStage;
        public string DeathAnimLib = "PED";
        public string DeathAnimName = "FLOOR_HIT";
        public int Cough, CoughTime;
        public int Migrain, MigrainTime, MigrainUsed;
        public int Fever, FeverTime, FeverUsed;
        public int DrugTime, DrugUsed;
    }

    public class CharSettings
    {
        public Language Language { get; set; } = Language.ID;
        public int EnterExit = 4;
        public int DynamicObjectsPriority = 1;
        public int HBEMode;
        public bool ShowHealth;
        public bool ShowArmour;
        public bool ShowHunger = true;
        public bool ShowThirst = true;
        public bool ShowStress = true;
        public bool ShowTime = true;
        public bool ToggleJoinLog;
        public bool ToggleNews = true;
        public bool ToggleQuiz;
        public bool ToggleAdvertise = true;
        public bool ToggleUppercase = true;
        public bool ToggleStreamerMode;
        public bool ToggleChatOOC;
        public bool ToggleFamilyChat;
        public bool ToggleWTChat = true;
        public bool ToggleFacRadioChat;
        public bool TogglePrivateMessage = true;
        public bool ToggleAdminCmdLog = true;
        public bool ToggleAutoLowChat;
        public bool ToggleAutoHandbrake;
        public bool ToggleAutoChatAnimation;
        public bool ToggleSeatbeltHelmet;
        public bool ToggleAutoMaskLoggedIn;
    }

    public class CharJob
    {
        public string JobName = "";
        public string RegisterDate = "";
    }

    public class CharInfo
    {
        public string Username { get; set; } = "";
        public int Skin { get; set; }
        public int Gender { get; set; }
        public string BirthDate { get; set; } = "";
        public int Height { get; set; } = 150;
        public string Hair { get; set; } = "";
        public string Eye { get; set; } = "";
    }

    internal sealed class CharCreationData
    {
        public string Name { get; set; } = "";
        public string BirthDate { get; set; } = "-";
        public int Gender { get; set; }
        public int Height { get; set; } = 150;
        public string Hair { get; set; } = "Hitam";
        public string Eye { get; set; } = "";
        public int Skin { get; set; } = 2;
        public int WhichSpawn { get; set; }
    }

    internal sealed class RawCharRow
    {
        public string Citizen_id { get; set; } = "";
        public string Ucp { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Reg_date { get; set; } = "";
        public string Last_login { get; set; } = "";
        public int Verified_char { get; set; }
        public int Level { get; set; } = 1;
        public int Level_points { get; set; }

        public int Money { get; set; }
        public int Admin { get; set; }
        public int Mask_id { get; set; }
        public int Warn { get; set; }
        public int Paycheck { get; set; }
        public string? Position { get; set; }
        public string? Vitals { get; set; }
        public string? Playtime { get; set; }
        public string? Backpack { get; set; }
        public string? Phone { get; set; }
        public string? Jail_info { get; set; }
        public string? Ban_info { get; set; }
        public string? Condition { get; set; }
        public string? Settings { get; set; }
        public string? Jobs { get; set; }
        public string? Paychecks { get; set; }
        public string? Char_info { get; set; }
        public string? Delays { get; set; }
    }

    internal sealed class CharListItem
    {
        public string Citizen_id { get; set; } = "";
        public string Username { get; set; } = "";
        public int Level { get; set; }
        public string Last_login { get; set; } = "";
    }

    internal static class CharacterService
    {
        private const string Table = "players";
        private const int MaxChars = 25;

        private static readonly (int Skin, string Hair)[] MaleSkins =
        {
            (1,"Hitam"),(2,"Hitam"),(3,"Hitam"),(4,"Hitam"),(5,"Hitam"),
            (6,"Hitam"),(7,"Hitam"),(14,"Hitam"),(29,"Hitam"),(100,"Hitam"),(299,"Hitam")
        };

        private static readonly (int Skin, string Hair)[] FemaleSkins =
        {
            (9,"Hitam"),(10,"Hitam"),(11,"Hitam"),(12,"Hitam"),(13,"Hitam"),
            (31,"Hitam"),(38,"Hitam"),(39,"Hitam"),(40,"Hitam"),(41,"Hitam")
        };

        private static readonly Regex _rpName = new(@"^[A-Z][a-z]+_[A-Z][a-z]+$", RegexOptions.Compiled);
        private static readonly JsonSerializerOptions _jOpts = new() { PropertyNameCaseInsensitive = true, IncludeFields = true };
        private static readonly Dictionary<int, List<CharListItem>> _lists = new();
        private static readonly Dictionary<int, CharCreationData> _creations = new();

        private static Language Lang(Player p) => p.IsCharLoaded ? p.Settings.Language : Language.ID;
        private static string L(Player p, string s, string k) => LocalizationManager.Get(Lang(p), s, k);
        private static string L(Player p, string s, string k, params object[] a) => LocalizationManager.Get(Lang(p), s, k, a);

        public static async void CheckPlayerCharAsync(Player player)
        {
            try
            {
                var rows = await DatabaseManager.QueryAsync<CharListItem>(
                    $"SELECT citizen_id, JSON_UNQUOTE(JSON_EXTRACT(char_info, '$.Username')) AS username, level, last_login FROM `{Table}` WHERE ucp = @Ucp LIMIT {MaxChars}",
                    new { Ucp = player.Name });

                if (player.IsDisposed) return;

                var list = new List<CharListItem>(rows);
                _lists[player.Id] = list;
                ShowCharListDialog(player, list);
            }
            catch (Exception ex) { Console.WriteLine($"[Character] CheckPlayerChar: {ex.Message}"); }
        }

        public static void HandleSpawn(Player player)
        {
            if (!player.IsCharLoaded) return;

            const int SpawnCooldownMs = 2000;
            var currentTick = Environment.TickCount;

            if (currentTick - player.LastSpawnTick < SpawnCooldownMs)
                return;

            player.LastSpawnTick = currentTick;

            if (BanService.IsPlayerBanned(player))
                return;

            player.ToggleControllableSafe(true);
            player.Score = player.Level;
            player.Color = Color.White;
            player.Name = player.CharInfo.Username;

            for (var i = 0; i < 50; i++) player.SendClientMessage(Color.White, "");
            player.SendClientMessage(Color.White, L(player, "CHAR", "WELCOME_1"));
            player.SendClientMessage(Color.White, L(player, "CHAR", "WELCOME_2", player.CharInfo.Username));
            player.SendClientMessage(Color.White, L(player, "CHAR", "WELCOME_3"));
            player.SendClientMessage(Color.White, L(player, "CHAR", "WELCOME_LAST_LOGIN", player.LastLogin));

            if (player.Settings.ToggleJoinLog)
            {
                var (city, country) = GeoLocationService.GetLocation(player.IP);
                var connectMessage = $"{{FFFF00}}** {{FF0000}}{player.CharInfo.Username} {{FFFF00}}telah terkoneksi ke server. ({city}, {country})";

                foreach (var p in SampSharp.GameMode.World.BasePlayer.All.OfType<Player>())
                {
                    if (p.IsCharLoaded && p.Settings.ToggleJoinLog)
                        p.SendClientMessage(Color.Yellow, connectMessage);
                }
            }

            RealtimeClockService.OnPlayerSpawn(player.Id, player.Settings.ShowTime);
            EnterExitService.ProcessEnterExit(player, () =>
            {
                if (!player.IsDisposed)
                {
                    NameTagService.Refresh(player);
                    SettingsService.ApplyDynamicObjectPriority(player);
                    JailService.OnPlayerSpawn(player);
                    PlaytimeService.RegisterPlayer(player);
                    PaycheckService.RegisterPlayer(player);

                    NeedsService.OnPlayerSpawn(player);

                    ConditionService.RegisterPlayer(player);
                    ConditionService.RestoreDeathState(player);

                    if (player.Condition.Injured == 0)
                    {
                        player.SetHealthSafe(player.Vitals.Health, player.Vitals.Armour);
                    }

                    player.ToggleControllableSafe(true);
                }
            });
        }

        public static async Task SaveAsync(Player player)
        {
            if (!player.IsCharLoaded) return;

            player.Vitals.Health = player.GetHealthSafe();
            player.Vitals.Armour = player.GetArmourSafe();

            var pos = new CharPosition
            {
                X = player.Position.X,
                Y = player.Position.Y,
                Z = player.Position.Z,
                A = player.Angle,
                Interior = player.Interior,
                World = player.GetVirtualWorldSafe()
            };

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET " +
                "level=@Level, level_points=@LevelPoints, " +
                "money=@Money, admin=@Admin, mask_id=@MaskId, warn=@Warn, " +
                "ip=@Ip, last_login=CURRENT_TIMESTAMP(), " +
                "position=@Pos, vitals=@Vitals, playtime=@Playtime, " +
                "backpack=@Backpack, phone=@Phone, jail_info=@JailInfo, ban_info=@BanInfo, " +
                "`condition`=@Condition, `settings`=@Settings, jobs=@Jobs, paychecks=@PaycheckData, " +
                "char_info=@CharInfo, delays=@Delays " +
                "WHERE citizen_id=@CitizenId",
                new
                {
                    player.Level,
                    LevelPoints = player.LevelPoints,
                    Money = player.CharMoney,
                    player.Admin,
                    player.MaskId,
                    player.Warn,
                    Ip = player.IP,
                    Pos = Ser(pos),
                    Vitals = Ser(player.Vitals),
                    Playtime = Ser(player.Playtime),
                    Backpack = Ser(player.InventoryData),
                    Phone = Ser(player.Phone),
                    JailInfo = Ser(player.JailInfo),
                    BanInfo = Ser(player.BanInfo),
                    Condition = Ser(player.Condition),
                    Settings = Ser(player.Settings),
                    Jobs = Ser(player.Jobs),
                    PaycheckData = Ser(player.PaycheckData),
                    CharInfo = Ser(player.CharInfo),
                    Delays = Ser(player.Delays),
                    CitizenId = player.CitizenId
                });
        }

        public static void Cleanup(Player player)
        {
            _lists.Remove(player.Id);
            _creations.Remove(player.Id);
            player.IsCharLoaded = false;
            player.LastSpawnTick = 0;
            PlaytimeService.UnregisterPlayer(player);
            PaycheckService.UnregisterPlayer(player);
        }

        private static void ShowCharListDialog(Player player, List<CharListItem> list)
        {
            var rows = new List<string[]>();
            foreach (var c in list)
                rows.Add(new[] { $"{{ffffff}}{c.Username}", c.Level.ToString(), c.Last_login });
            if (list.Count < MaxChars)
                rows.Add(new[] { L(player, "CHAR", "LIST_CREATE_BTN"), "", "" });

            player.ShowTabList(
                L(player, "CHAR", "LIST_TITLE"),
                new[] { L(player, "CHAR", "LIST_COL_NAME"), L(player, "CHAR", "LIST_COL_LEVEL"), L(player, "CHAR", "LIST_COL_LOGIN") })
                .WithRows(rows.ToArray())
                .WithButtons(L(player, "GENERAL", "BTN_SELECT"), L(player, "GENERAL", "BTN_QUIT"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { player.Kick(); return; }
                    if (!_lists.TryGetValue(player.Id, out var l)) return;
                    if (e.ListItem < l.Count) LoadExistingCharAsync(player, l[e.ListItem].Citizen_id);
                    else ShowCreateNameDialog(player);
                });
        }

        private static void ShowCreateNameDialog(Player player, bool taken = false)
        {
            var body = taken
                ? L(player, "CHAR", "CREATE_NAME_TAKEN_MSG")
                : L(player, "CHAR", "CREATE_NAME_MSG");

            player.ShowInput(L(player, "CHAR", "CREATE_NAME_TITLE"), body)
                .WithButtons(L(player, "GENERAL", "BTN_CREATE"), L(player, "GENERAL", "BTN_EXIT"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { player.Kick(); return; }
                    HandleCreateNameAsync(player, e.InputText);
                });
        }

        private static async void HandleCreateNameAsync(Player player, string name)
        {
            try
            {
                if (name.Length < 1 || name.Length > 24 || !_rpName.IsMatch(name))
                {
                    ShowCreateNameDialog(player);
                    return;
                }

                var taken = await DatabaseManager.ExistsAsync(
                    $"SELECT COUNT(*) FROM `{Table}` WHERE JSON_UNQUOTE(JSON_EXTRACT(char_info, '$.Username')) = @Name", new { Name = name });

                if (player.IsDisposed) return;
                if (taken) { ShowCreateNameDialog(player, taken: true); return; }

                _creations[player.Id] = new CharCreationData { Name = name };
                EnterCreationScene(player);
                ShowSettingsDialog(player);
            }
            catch (Exception ex) { Console.WriteLine($"[Character] HandleCreateName: {ex.Message}"); }
        }

        private static void EnterCreationScene(Player player)
        {
            CinematicCameraService.Stop(player);
            player.SetInteriorSafe(14);
            player.SetVirtualWorldSafe(player.Id);

            var pos = new Vector3(255.306320f, -41.813072f, 1002.023437f);
            player.SetPositionSafe(pos);
            player.Angle = 257.241699f;
            player.CameraPosition = pos;
            player.SetCameraLookAt(pos, CameraCut.Cut);

            StreamerNatives.Instance.Streamer_UpdateEx(player.Id, pos.X, pos.Y, pos.Z);
            StreamerNatives.Instance.Streamer_Update(player.Id);
        }

        private static void ShowSettingsDialog(Player player)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;

            player.ShowTabList(
                L(player, "CHAR", "SETTINGS_TITLE"),
                new[] { L(player, "CHAR", "SETTINGS_COL_DATA"), L(player, "CHAR", "SETTINGS_COL_VALUE") })
                .WithRows(new[] {
                    new[] { L(player, "CHAR", "SETTINGS_ROW_NAME"),      $"{{ffffff}}{c.Name}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_BIRTHDATE"), $"{{ffffff}}{c.BirthDate}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_GENDER"),    $"{{ffffff}}{FmtGender(player, c.Gender)}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_HEIGHT"),    $"{{ffffff}}{c.Height} cm" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_HAIR"),      $"{{ffffff}}{c.Hair}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_EYE"),       $"{{ffffff}}{c.Eye}" },
                    new[] { L(player, "CHAR", "SETTINGS_ROW_CREATE"),    "" }
                })
                .WithButtons(L(player, "GENERAL", "BTN_SELECT"), L(player, "GENERAL", "BTN_CANCEL"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        _lists.TryGetValue(player.Id, out var l);
                        ShowCharListDialog(player, l ?? new());
                        return;
                    }
                    HandleSettingItem(player, e.ListItem);
                });
        }

        private static void HandleSettingItem(Player player, int item)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;
            switch (item)
            {
                case 0:
                    player.SendClientMessage(Color.White, L(player, "CHAR", "SETTINGS_ERR_NAME"));
                    ShowSettingsDialog(player);
                    break;
                case 1: ShowBirthDateDialog(player); break;
                case 2: ShowGenderDialog(player); break;
                case 3: ShowHeightDialog(player); break;
                case 4:
                    player.SendClientMessage(Color.White, L(player, "CHAR", "SETTINGS_ERR_HAIR"));
                    ShowSettingsDialog(player);
                    break;
                case 5: ShowEyeColorDialog(player); break;
                case 6:
                    if (c.BirthDate is "-" or { Length: <= 1 })
                    { player.SendClientMessage(Color.White, L(player, "CHAR", "SETTINGS_ERR_BIRTHDATE")); ShowSettingsDialog(player); return; }
                    if (string.IsNullOrWhiteSpace(c.Eye))
                    { player.SendClientMessage(Color.White, L(player, "CHAR", "SETTINGS_ERR_EYE")); ShowSettingsDialog(player); return; }
                    ShowSpawnSelectorDialog(player);
                    break;
            }
        }

        private static void ShowBirthDateDialog(Player player, string? errKey = null)
        {
            var body = L(player, "CHAR", "BIRTHDATE_MSG");
            if (errKey != null) body += $"\n{{ff0000}}(!) " + L(player, "CHAR", errKey);

            player.ShowInput(L(player, "CHAR", "BIRTHDATE_TITLE"), body)
                .WithButtons(L(player, "GENERAL", "BTN_SELECT"), L(player, "GENERAL", "BTN_CANCEL"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSettingsDialog(player); return; }
                    HandleBirthDate(player, e.InputText);
                });
        }

        private static void HandleBirthDate(Player player, string input)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;
            var parts = input.Split('/');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var d) ||
                !int.TryParse(parts[1], out var m) || !int.TryParse(parts[2], out var y))
            { ShowBirthDateDialog(player, "BIRTHDATE_ERR_INVALID"); return; }

            int[] mDays = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            if (y < 1900 || y > DateTime.Now.Year) { ShowBirthDateDialog(player, "BIRTHDATE_ERR_YEAR"); return; }
            if (m < 1 || m > 12) { ShowBirthDateDialog(player, "BIRTHDATE_ERR_MONTH"); return; }
            if (d < 1 || d > mDays[m - 1]) { ShowBirthDateDialog(player, "BIRTHDATE_ERR_DAY"); return; }

            c.BirthDate = input;
            ShowSettingsDialog(player);
        }

        private static void ShowGenderDialog(Player player)
        {
            player.ShowList(
                L(player, "CHAR", "GENDER_TITLE"),
                L(player, "CHAR", "GENDER_MALE"),
                L(player, "CHAR", "GENDER_FEMALE"))
                .WithButtons(L(player, "GENERAL", "BTN_INPUT"), L(player, "GENERAL", "BTN_CANCEL"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSettingsDialog(player); return; }
                    if (!_creations.TryGetValue(player.Id, out var c)) return;
                    c.Gender = e.ListItem;
                    ShowSkinDialog(player);
                });
        }

        private static void ShowSkinDialog(Player player)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;
            var skinList = c.Gender == 1 ? FemaleSkins : MaleSkins;
            var items = new List<PreviewModelItem>();
            foreach (var s in skinList)
                items.Add(new PreviewModelItem { ModelId = s.Skin, Text = $"ID: {s.Skin}" });

            PreviewModelDialog.Show(player, 0, FmtGender(player, c.Gender), items,
                L(player, "GENERAL", "BTN_SELECT"), L(player, "GENERAL", "BTN_CANCEL"),
                args =>
                {
                    if (!args.Accepted) { ShowGenderDialog(player); return; }
                    if (!_creations.TryGetValue(player.Id, out var cc)) return;
                    var sk = (cc.Gender == 1 ? FemaleSkins : MaleSkins)[args.ListItem];
                    cc.Skin = sk.Skin;
                    cc.Hair = sk.Hair;
                    player.Skin = cc.Skin;
                    ShowSettingsDialog(player);
                });
        }

        private static void ShowHeightDialog(Player player, string? errKey = null)
        {
            var body = L(player, "CHAR", "HEIGHT_MSG");
            if (errKey != null) body += $"\n{{ff0000}}(!) " + L(player, "CHAR", errKey);

            player.ShowInput(L(player, "CHAR", "HEIGHT_TITLE"), body)
                .WithButtons(L(player, "GENERAL", "BTN_INPUT"), L(player, "GENERAL", "BTN_CANCEL"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSettingsDialog(player); return; }
                    HandleHeight(player, e.InputText);
                });
        }

        private static void HandleHeight(Player player, string input)
        {
            if (!_creations.TryGetValue(player.Id, out var c)) return;
            if (!int.TryParse(input, out var h)) { ShowHeightDialog(player, "HEIGHT_ERR_NAN"); return; }
            if (h < 130 || h > 195) { ShowHeightDialog(player, "HEIGHT_ERR_RANGE"); return; }
            c.Height = h;
            ShowSettingsDialog(player);
        }

        private static void ShowEyeColorDialog(Player player)
        {
            player.ShowList(
                L(player, "CHAR", "EYE_TITLE"),
                L(player, "CHAR", "EYE_BLACK"),
                L(player, "CHAR", "EYE_BROWN"),
                L(player, "CHAR", "EYE_BLUE"),
                L(player, "CHAR", "EYE_GRAY"))
                .WithButtons(L(player, "GENERAL", "BTN_SELECT"), L(player, "GENERAL", "BTN_CANCEL"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSettingsDialog(player); return; }
                    if (!_creations.TryGetValue(player.Id, out var c)) return;
                    c.Eye = e.ListItem switch { 0 => "Hitam", 1 => "Coklat", 2 => "Biru", _ => "Abu-abu Muda" };
                    ShowSettingsDialog(player);
                });
        }

        private static void ShowSpawnSelectorDialog(Player player)
        {
            player.ShowList(
                L(player, "CHAR", "SPAWN_TITLE"),
                L(player, "CHAR", "SPAWN_AIRPORT"),
                L(player, "CHAR", "SPAWN_STATION"))
                .WithButtons(L(player, "CHAR", "SPAWN_BTN"), "")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { ShowSpawnSelectorDialog(player); return; }
                    if (!_creations.TryGetValue(player.Id, out var c)) return;
                    c.WhichSpawn = e.ListItem;
                    RegisterNewCharAsync(player);
                });
        }

        private static async void RegisterNewCharAsync(Player player)
        {
            try
            {
                if (!_creations.TryGetValue(player.Id, out var c)) return;

                string cid;
                do { cid = GenCitizenId(); }
                while (await DatabaseManager.ExistsAsync(
                    $"SELECT COUNT(*) FROM `{Table}` WHERE citizen_id = @Id", new { Id = cid }));

                if (player.IsDisposed) return;

                var pos = c.WhichSpawn == 0
                    ? new CharPosition { X = 1685.602172f, Y = -2239.236572f, Z = 13.546875f, A = 182.176986f }
                    : new CharPosition { X = 1773.370117f, Y = -1936.799072f, Z = 13.552585f, A = 322.835693f };

                var maskId = new Random().Next(111111, 988888);

                await DatabaseManager.ExecuteAsync(
                    $"INSERT INTO `{Table}` " +
                    "(citizen_id,ucp,ip,mask_id," +
                    "position,vitals,playtime,backpack,phone,jail_info,ban_info,`condition`,`settings`,jobs,paychecks,char_info, delays) " +
                    "VALUES (@Cid,@Ucp,@Ip,@MaskId," +
                    "@Pos,@Vitals,@Playtime,@Backpack,@Phone,@JailInfo,@BanInfo,@Condition,@Settings,@Jobs,@PaycheckData,@CharInfo,@Delays)",
                    new
                    {
                        Cid = cid,
                        Ucp = player.Name,
                        Ip = player.IP,
                        MaskId = maskId,
                        Pos = Ser(pos),
                        Vitals = Ser(new CharVitals()),
                        Playtime = Ser(new CharPlaytime()),
                        Backpack = Ser(new InventoryData()),
                        Phone = Ser(new CharPhone()),
                        JailInfo = Ser(new CharJailInfo()),
                        BanInfo = Ser(new CharBanInfo()),
                        Condition = Ser(new CharCondition()),
                        Settings = Ser(new CharSettings()),
                        Jobs = Ser(new List<CharJob>()),
                        PaycheckData = Ser(new PaycheckData()),
                        CharInfo = Ser(new CharInfo
                        {
                            Username = c.Name,
                            Skin = c.Skin,
                            Gender = c.Gender,
                            BirthDate = c.BirthDate,
                            Height = c.Height,
                            Hair = c.Hair,
                            Eye = c.Eye
                        }),
                        Delays = Ser(new CharDelays()),
                    });

                if (player.IsDisposed) return;

                ApplyToPlayer(player, new RawCharRow
                {
                    Citizen_id = cid,
                    Ucp = player.Name,
                    Ip = player.IP,
                    Mask_id = maskId,
                    Position = Ser(pos),
                    Level = 1,
                    Last_login = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Char_info = Ser(new CharInfo
                    {
                        Username = c.Name,
                        Skin = c.Skin,
                        Gender = c.Gender,
                        BirthDate = c.BirthDate,
                        Height = c.Height,
                        Hair = c.Hair,
                        Eye = c.Eye
                    })
                });

                _creations.Remove(player.Id);
                SpawnCharacter(player);
            }
            catch (Exception ex) { Console.WriteLine($"[Character] RegisterNewChar: {ex.Message}"); }
        }

        private static async void LoadExistingCharAsync(Player player, string citizenId)
        {
            try
            {
                var raw = await DatabaseManager.QueryFirstAsync<RawCharRow>(
                    $"SELECT * FROM `{Table}` WHERE citizen_id = @Id LIMIT 1", new { Id = citizenId });

                if (player.IsDisposed) return;

                if (raw is null)
                {
                    _lists.TryGetValue(player.Id, out var l);
                    ShowCharListDialog(player, l ?? new());
                    return;
                }

                ApplyToPlayer(player, raw);

                await DatabaseManager.ExecuteAsync(
                    $"UPDATE `{Table}` SET ip=@Ip, last_login=CURRENT_TIMESTAMP() WHERE citizen_id=@Id",
                    new { Ip = player.IP, Id = raw.Citizen_id });

                if (player.IsDisposed) return;
                SpawnCharacter(player);
            }
            catch (Exception ex) { Console.WriteLine($"[Character] LoadExistingChar: {ex.Message}"); }
        }
        private static void SpawnCharacter(Player player)
        {
            CinematicCameraService.Stop(player);
            player.ToggleSpectatingSafe(false);
            player.SetInteriorSafe(player.CharSpawnPos.Interior);
            player.SetVirtualWorldSafe(player.CharSpawnPos.World);
            player.SetSpawnInfoSafe(0, player.CharInfo.Skin, player.CharSpawnPos.X, player.CharSpawnPos.Y, player.CharSpawnPos.Z, player.CharSpawnPos.A);
            player.SpawnPlayerSafe();
        }
        public static void RespawnCharacter(Player player)
        {
            if (!player.IsCharLoaded) return;
            var pos = player.CharSpawnPos;
            player.SetInteriorSafe(pos.Interior);
            player.SetVirtualWorldSafe(pos.World);
            player.SetSpawnInfoSafe(0, player.CharInfo.Skin, pos.X, pos.Y, pos.Z, pos.A);
            player.ToggleSpectatingSafe(false);
        }

        private static void ApplyToPlayer(Player player, RawCharRow r)
        {
            player.CitizenId = r.Citizen_id;
            player.Ucp = r.Ucp;
            player.RegDate = r.Reg_date;
            player.LastLogin = r.Last_login;
            player.VerifiedChar = r.Verified_char;
            player.Level = r.Level;
            player.LevelPoints = r.Level_points;
            player.CharMoney = r.Money;
            player.Admin = r.Admin;
            player.MaskId = r.Mask_id;
            player.Warn = r.Warn;
            player.CharInfo = Des<CharInfo>(r.Char_info) ?? new();
            player.Vitals = Des<CharVitals>(r.Vitals) ?? new();
            player.Playtime = Des<CharPlaytime>(r.Playtime) ?? new();

            player.InventoryData = Des<InventoryData>(r.Backpack) ?? new();
            player.Phone = Des<CharPhone>(r.Phone) ?? new();
            player.JailInfo = Des<CharJailInfo>(r.Jail_info) ?? new();
            player.BanInfo = Des<CharBanInfo>(r.Ban_info) ?? new();
            player.CharSpawnPos = Des<CharPosition>(r.Position) ?? new();
            player.Condition = Des<CharCondition>(r.Condition) ?? new();
            player.Settings = Des<CharSettings>(r.Settings) ?? new();
            player.Jobs = Des<List<CharJob>>(r.Jobs) ?? new();
            player.PaycheckData = Des<PaycheckData>(r.Paychecks) ?? new();
            player.Delays = Des<CharDelays>(r.Delays) ?? new();
            player.IsCharLoaded = true;
        }

        private static string FmtGender(Player p, int g) => g == 0 ? L(p, "CHAR", "GENDER_MALE") : L(p, "CHAR", "GENDER_FEMALE");
        private static string Ser<T>(T obj) => JsonSerializer.Serialize(obj, _jOpts);

        private static string GenCitizenId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var buf = new char[8];
            for (var i = 0; i < 8; i++)
                buf[i] = chars[System.Security.Cryptography.RandomNumberGenerator.GetInt32(chars.Length)];
            return new string(buf);
        }

        private static T? Des<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json, _jOpts); }
            catch { return null; }
        }
    }
}