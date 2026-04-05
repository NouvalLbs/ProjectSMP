using Microsoft.VisualBasic;
using ProjectSMP.Core;
using ProjectSMP.Features.Bank;
using ProjectSMP.Features.Chat;
using ProjectSMP.Features.Jobs.Core;
using ProjectSMP.Features.LevelSystem;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System;
using System.Linq;

namespace ProjectSMP.Commands
{
    public class GeneralCommands
    {
        [Command("b")]
        public static void LocalOOC(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /b [Text]");
                return;
            }

            var msg = text;
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            if (msg.Length > 64)
            {
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.LocalOOC),
                    $"{Utilities.ReturnName(player)}: (( {msg.Substring(0, 64)} ..");
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.LocalOOC),
                    $".. {msg.Substring(64)} ))");
            }
            else
            {
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.LocalOOC),
                    $"{Utilities.ReturnName(player)} says: (( {msg} ))");
            }
        }

        [Command("ooc", Shortcut = "o")]
        public static void GlobalOOC(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /o(oc) [Text]");
                return;
            }

            if (text.Length > 90)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Teks yang kamu masukan terlalu panjang, maksimal 90 karakter!");
                return;
            }

            var msg = text;
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            string formatted;
            if (player.AdminOnDuty)
            {
                formatted = $"(( {{ff0000}}Admin {player.Ucp}: {{ffffff}}{msg} {{E0FFFF}}))";
            }
            else
            {
                formatted = $"(( {player.Ucp} [{player.Id}]: {{ffffff}}{msg} ))";
            }

            foreach (var p in BasePlayer.All)
            {
                if (p is Player target && target.IsCharLoaded && target.Settings.ToggleChatOOC)
                {
                    target.SendClientMessage(new Color(ChatColors.GlobalOOC), formatted);
                }
            }
        }

        [Command("ab")]
        public static void AboveHead(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /ab [Text]");
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan '/ab off' untuk menonaktifkan atau menghapus tag ab.");
                return;
            }

            if (text.Length > 128)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Teks maksimal hanya bisa sampai 128 karakter.");
                return;
            }

            if (text.Trim().Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                if (!ChatService.Is3DLabelActive(player, false))
                {
                    player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum mengaktifkan teks '{{ffea00}}ab{{FFFFFF}}'.");
                    return;
                }

                ChatService.Remove3DLabel(player, false);
                player.SendClientMessage(Color.White, $"{Msg.AB}  Kamu telah menghapus teks '{{ffea00}}ab{{FFFFFF}}'.");
                return;
            }

            var msg = ChatService.MessageFix(text);
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            var labelText = $"* {Utilities.ReturnName(player)} *\n(( OOC : {msg} ))";
            ChatService.CreateOrUpdate3DLabel(player, false, labelText, new Color(ChatColors.Purple));
            player.SendClientMessage(Color.White, $"{Msg.AB}  Teks telah ditempatkan di lokasimu, untuk menghapusnya gunakan '{{ffea00}}/ab off{{FFFFFF}}'.");
        }

        [Command("low", Shortcut = "l")]
        public static void Low(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /l(ow) [Text]");
                return;
            }
            ChatService.TalkMessage(ChatDistance.Low, player, "whispers", text);
        }

        [Command("shout", Shortcut = "s")]
        public static void Shout(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /s(hout) [Text]");
                return;
            }
            ChatService.TalkMessage(ChatDistance.Shout, player, "shouts", text);
            player.ApplyAnimation("ON_LOOKERS", "shout_01", 4.0f, false, false, false, false, 0);
        }

        [Command("whisper", Shortcut = "w")]
        public static void Whisper(Player player, string targetInput, string text)
        {
            if (string.IsNullOrWhiteSpace(targetInput) || string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /w(hisper) [PlayerId/PartOfName] [Text]");
                return;
            }

            var target = Utilities.GetPlayerFromPartOfName(player, targetInput);
            if (target == null) return;

            if (target.Id == player.Id)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu tidak dapat melakukan Whisper kepada dirimu sendiri.");
                return;
            }

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Player tersebut tidak ada di kota saat ini.");
                return;
            }

            ChatService.ProcessWhisper(player, target, text);
        }

        [Command("me")]
        public static void Me(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /me [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.Me, ChatDistance.Normal);
        }

        [Command("lowerme", Shortcut = "lme")]
        public static void LowerMe(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /l(ower)me [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.LowerMe, ChatDistance.Low);
        }

        [Command("ame")]
        public static void Ame(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /ame [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.Ame, ChatDistance.Low);
        }

        [Command("do")]
        public static void Do(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /do [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.Do, ChatDistance.Normal);
        }

        [Command("lowerdo", Shortcut = "ldo")]
        public static void LowerDo(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /l(ower)do [Text Action]");
                return;
            }
            ChatService.ProcessActionText(player, text, ActionType.LowerDo, ChatDistance.Low);
        }

        [Command("ado")]
        public static void Ado(Player player, string header, string description = "")
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /ado [Text Header] [Text]");
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan '/ado off' untuk menonaktifkan atau menghapus tag ado.");
                return;
            }

            if (header.Trim().Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                if (!ChatService.Is3DLabelActive(player, true))
                {
                    player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum mengaktifkan teks '{{ffea00}}/ado{{FFFFFF}}'.");
                    return;
                }

                ChatService.Remove3DLabel(player, true);
                player.SendClientMessage(Color.White, $"{Msg.ADO}  Kamu telah menghapus teks '{{ffea00}}/ado{{FFFFFF}}'.");
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu harus memasukkan deskripsi setelah header!");
                player.SendClientMessage(Color.White, $"{Msg.Command} Contoh: /ado [Toko Buka] Silakan masuk dan belanja");
                return;
            }

            var desc = ChatService.MessageFix(description);
            if (player.Settings.ToggleUppercase && desc.Length > 0)
                desc = char.ToUpper(desc[0]) + desc.Substring(1);

            var labelText = $"* [{header}] {desc} *\n(( {Utilities.ReturnName(player)} ))";
            ChatService.CreateOrUpdate3DLabel(player, true, labelText, new Color(ChatColors.Purple));
            player.SendClientMessage(Color.White, $"{Msg.ADO}  Teks telah ditempatkan di lokasimu, untuk menghapusnya gunakan '{{ffea00}}/ado off{{FFFFFF}}'.");
        }

        [Command("try")]
        public static void Try(Player player, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /try [Text Action]");
                return;
            }

            var msg = text;
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            var result = new Random().Next(2) == 0 ? "and success" : "but fail";

            if (msg.Length > 64)
            {
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.Purple),
                    $"* {Utilities.ReturnName(player)} {msg.Substring(0, 64)} ..");
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.Purple),
                    $".. {msg.Substring(64)}, {result}");
            }
            else
            {
                ChatService.SendNearbyMessage(player, 20f, new Color(ChatColors.Purple),
                    $"* {Utilities.ReturnName(player)} {msg}, {result}");
            }
        }

        [Command("pm")]
        public static void PrivateMessage(Player player, string targetInput, string text)
        {
            if (string.IsNullOrWhiteSpace(targetInput) || string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /pm [PlayerId/PartOfName] [Text]");
                return;
            }

            var target = Utilities.GetPlayerFromPartOfName(player, targetInput);
            if (target == null) return;

            if (target.Id == player.Id)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu tidak dapat melakukan PM kepada dirimu sendiri.");
                return;
            }

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Player tersebut tidak ada di kota saat ini.");
                return;
            }

            if (!target.Settings.TogglePrivateMessage)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Player tersebut menonaktifkan private message.");
                return;
            }

            ChatService.ProcessPrivateMessage(player, target, text);
        }

        [Command("help")]
        public static void Help(Player player)
        {
            player.ShowList("General Help", "General Commands", "Chatting Commands")
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;
                    if (e.ListItem == 0) ShowGeneralHelp(player);
                    else ShowChatHelp(player);
                });
        }

        [Command("clearchat", Shortcut = "clear")]
        public static void ClearChat(Player player)
        {
            for (var i = 0; i < 50; i++)
                player.SendClientMessage(Color.White, "");
        }

        [Command("tte2")]
        public static void TestUcok(Player player)
        {
            var strings = TextFormatter.BuildWithNewLines(
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, nisl vel tincidunt luctus, nunc nisl",
                "aliquam nunc, eget aliquam nisl nunc vel nisl. Proin euismod, nisl vel tincidunt luctus, nunc nisl aliquam nunc,",
                "eget aliquam nisl nunc vel nisl. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi.",
                "Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris.",
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, nisl vel tincidunt luctus, nunc nisl",
                "aliquam nunc, eget aliquam nisl nunc vel nisl. Proin euismod, nisl vel tincidunt luctus, nunc nisl aliquam nunc,",
                "eget aliquam nisl nunc vel nisl. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi.",
                "Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris.",
                "Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent maurisssss."
            );
            Console.WriteLine(strings.Length);
            var status = new MessageDialog("Title", strings, "Test", "Test");
            status.Show(player);
        }

        [Command("stats")]
        public static void Stats(Player player)
        {
            var gender = player.CharInfo.Gender == 0 ? "Male" : "Female";
            var phoneStatus = player.Phone.Off == 0 ? "{FF0000}Offline{FFFFFF}" : "{91ff00}Online{FFFFFF}";
            var charStatus = player.VerifiedChar == 1 ? "{91ff00}Verified{FFFFFF}" : "{FF0000}Unverified{FFFFFF}";
            var admin = Utilities.GetAdminString(player);
            var warn = Utilities.GetWarningString(player);
            var jobs = JobService.GetAllJobsString(player);
            var pointsRequired = LevelService.GetPointsRequired(player.Level);

            var activeAccount = player.BankAccounts.FirstOrDefault(a => a.IsActive);
            var bankText = activeAccount != null
                ? $"{{00f000}}{Utilities.GroupDigits(player.BankAccounts.Where(a => a.IsActive).Sum(a => a.Balance))}{{FFFFFF}}] | Bank Account: [{{b8d2ec}}{activeAccount.AccountNumber}{{FFFFFF}}"
                : "{FF0000}Unregistered{FFFFFF}";

            var stats = TextFormatter.BuildWithNewLines(
                "{FFFF00}IC Information:",
                $"{{FFFFFF}}Gender: [{{b8d2ec}}{gender}{{FFFFFF}}] | Birthdate: [{{b8d2ec}}{player.CharInfo.BirthDate}{{FFFFFF}}] | Money: [{{00f000}}{Utilities.GroupDigits(player.CharMoney)}{{FFFFFF}}] | Bank: [{bankText}]",
                $"{{FFFFFF}}Phone Status: [{phoneStatus}] | Phone Number: [{{ebeb00}}{player.Phone.Number}{{FFFFFF}}] | Phone Credit: [{{ebeb00}}{player.Phone.Credit}{{FFFFFF}}] | Mask ID: [{{b8d2ec}}{player.MaskId}{{FFFFFF}}]",
                $"{{FFFFFF}}Jobs: [{jobs}{{FFFFFF}}] | Faction: [Civilian{{FFFFFF}}] | Family: [None]",
                $"{{FFFFFF}}Working at: [None] [None (0){{FFFFFF}}] | Wealth: [None]",
                "",
                "{FFFF00}OOC Information:",
                $"{{FFFFFF}}CitizenId: [{{77efc7}}{player.CitizenId}{{FFFFFF}}] | Level: [{{77efc7}}{player.Level} - ({player.LevelPoints}/{pointsRequired}){{FFFFFF}}] | Paychecks: [{{b8d2ec}}{player.PaycheckData.PaycheckTime}{{FFFFFF}}] | Time Played: [{{b8d2ec}}{player.Playtime.Hours} hour(s) {player.Playtime.Minutes} minute(s) {player.Playtime.Seconds} second(s){{FFFFFF}}]",
                $"{{FFFFFF}}Character Story: [{charStatus}] | Staff: [{admin}] | Warns: [{warn}] | Prestige Coin: [0]",
                $"{{FFFFFF}}World: [{{ebeb00}}{player.VirtualWorld}{{FFFFFF}}] | Interior: [{{ebeb00}}{player.Interior}{{FFFFFF}}] | MaxHP: [{{ab0000}}{player.Vitals.MaxHealth:F1}{{FFFFFF}}] | Health: [{{ab0000}}{player.Vitals.Health:F1}{{FFFFFF}}] | Armour: [{{9f9f9f}}{player.Vitals.Armour:F1}{{FFFFFF}}]"
            );

            Console.WriteLine(stats.Length);
            var title = $"{{6fe0ba}}{player.CharInfo.Username} Statistic {{c8c8c8}}(UCP: {player.Ucp})";
            player.ShowMessage(title, stats).WithButtons("Settings", "Close").Show();
        }

        private static void ShowGeneralHelp(Player player)
        {
            player.ShowPagedTabList("General Player Commands", new[] { "Perintah", "Informasi" })
                .WithRows(
                    new[] { "/help", "Menampilkan daftar perintah bantuan" },
                    new[] { "/i(tems)", "Menampilkan daftar barang yang dimiliki character kamu" },
                    new[] { "/settings", "Mengubah pengaturan character kamu" },
                    new[] { "/hud", "Mengubah tampilan HUD character kamu" },
                    new[] { "/toggle", "Mengaktifkan atau menonaktifkan fitur tertentu" },
                    new[] { "/clear(chat)", "Menghapus semua pesan di chat" },
                    new[] { "/report", "Melaporkan masalah atau pelanggaran kepada admin" },
                    new[] { "/ask", "Bertanya kepada admin/helper yang sedang online" },
                    new[] { "/mysalary", "Menampilkan informasi gaji karakter kamu" }
                )
                .WithButtons("Close", "")
                .Show();
        }

        private static void ShowChatHelp(Player player)
        {
            player.ShowPagedTabList("Chatting Commands", new[] { "Perintah", "Informasi" })
                .WithRows(
                    new[] { "/b [text]", "Chat lokal yang digunakan sebagai komunikasi antar player" },
                    new[] { "/o [text]", "Chat global antar semua player yang online di server" },
                    new[] { "/ab [text]", "Menampilkan pesan label (Out Of Character)" },
                    new[] { "/l(ow) [text]", "Digunakan untuk berbicara dengan radius kecil" },
                    new[] { "/s(hout) [text]", "Digunakan untuk berteriak" },
                    new[] { "/w(hisper) [id] [text]", "Membisikkan sesuatu ke character yang ditentukan" },
                    new[] { "/me [text]", "Menampilkan aktivitas character kamu" },
                    new[] { "/l(ower)me [text]", "Menampilkan aktivitas character dengan radius kecil" },
                    new[] { "/ame [text]", "Menampilkan aktivitas di atas kepala character" },
                    new[] { "/do [text]", "Menampilkan keadaan atau situasi sekitar" },
                    new[] { "/l(ower)do [text]", "Menampilkan keadaan sekitar dengan radius kecil" },
                    new[] { "/ado [title] [text]", "Menampilkan pesan label keadaan character" },
                    new[] { "/try [text]", "Menampilkan percobaan tindakan character" },
                    new[] { "/pm [id] [text]", "Mengirim pesan pribadi ke player lain" }
                )
                .WithButtons("Close", "")
                .Show();
        }
    }
}