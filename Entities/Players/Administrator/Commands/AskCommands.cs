using ProjectSMP.Core;
using ProjectSMP.Extensions;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class AskCommands : AdminCommandBase
    {
        [Command("ask")]
        public static void Ask(Player player, string question)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /ask [pertanyaan]");
                return;
            }

            if (!AskService.CanAsk(player))
            {
                var cooldown = AskService.GetCooldown(player);
                if (cooldown > 0)
                {
                    player.SendClientMessage(Color.White, $"{Msg.Error} Tunggu {cooldown} detik sebelum bertanya lagi.");
                }
                else
                {
                    player.SendClientMessage(Color.White, $"{Msg.Error} Menunggu jawaban dari admin untuk mengulangi pertanyaan lagi..");
                }
                return;
            }

            AskService.AddAsk(player, question);
            player.SetData("LastAskQuestion", question);
            player.SendClientMessage(Color.White, "{C6E2FF}ASK:{FFFFFF} Kamu telah mengirim pertanyaan ke helper/admin yang online.");
        }

        [Command("asks")]
        public static void Asks(Player player)
        {
            if (!CheckAdmin(player, 1)) return;

            var asks = AskService.GetActiveAsks();
            if (asks.Count == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Tidak ada ask aktif saat ini.");
                return;
            }

            var rows = new List<string[]>();
            var askMapping = new Dictionary<int, int>();
            var listIndex = 0;

            foreach (var ask in asks)
            {
                var timeStr = ask.CreatedAt.ToString("HH:mm:ss");
                var askerStr = $"{{ffffff}}P{ask.PlayerId}: {{FFFF00}}{ask.PlayerName}";

                var question = ask.Question.Length > 50
                    ? ask.Question.Substring(0, 50) + "..."
                    : ask.Question;

                rows.Add(new[]
                {
                    askerStr,
                    $"{{ffffff}}{timeStr}",
                    $"{{ffffff}}{question}"
                });

                askMapping[listIndex] = ask.Id;
                listIndex++;
            }

            player.SetData("AskMapping", askMapping);
            player.ShowTabList("Ask Queue", new[] { "Asker", "Time", "Question" })
                .WithRows(rows.ToArray())
                .WithButtons("Answer", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        player.SetData<Dictionary<int, int>>("AskMapping", null);
                        return;
                    }

                    var mapping = player.GetData<Dictionary<int, int>>("AskMapping", null);
                    if (mapping == null || !mapping.TryGetValue(e.ListItem, out var askId))
                        return;

                    var selectedAsk = AskService.GetActiveAsks().Find(a => a.Id == askId);
                    if (selectedAsk == null)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Ask tidak ditemukan.");
                        return;
                    }

                    player.SetData("SelectedAskPlayerId", selectedAsk.PlayerId);

                    var body = $"{{FFFFFF}}Player: {{FFFF00}}{selectedAsk.PlayerName}\n" +
                               $"{{FFFFFF}}Question: {{45ddd7}}{selectedAsk.Question}\n" +
                               $"{{FFFFFF}}Answer: {{0dd118}}(Input below)";

                    player.ShowInput("Answer Question", body)
                        .WithButtons("Answer", "Back")
                        .Show(answerEvent =>
                        {
                            if (answerEvent.DialogButton != DialogButton.Left)
                            {
                                Asks(player);
                                return;
                            }

                            var targetId = player.GetData("SelectedAskPlayerId", -1);
                            if (targetId == -1 || string.IsNullOrWhiteSpace(answerEvent.InputText))
                            {
                                player.SendClientMessage(Color.White, $"{Msg.Error} Jawaban tidak valid.");
                                return;
                            }

                            AskService.AnswerAsk(player, targetId, answerEvent.InputText);
                            player.SetData("SelectedAskPlayerId", -1);
                        });
                });
        }

        [Command("ans")]
        public static void Ans(Player player, string targetInput, string answer)
        {
            if (!CheckAdmin(player, 1)) return;

            if (string.IsNullOrWhiteSpace(targetInput) || string.IsNullOrWhiteSpace(answer))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /ans [playerid/PartOfName] [jawaban]");
                return;
            }

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            var askCount = AskService.GetPlayerAskCount(target);
            if (askCount == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Player ini tidak bertanya apa-apa.");
                return;
            }

            AskService.AnswerAsk(player, target.Id, answer);
        }

        [Command("clearallasks")]
        public static void ClearAllAsks(Player player)
        {
            if (!CheckAdmin(player, 5)) return;

            var count = AskService.GetAskCount();
            if (count == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Tidak ada ask aktif untuk dihapus.");
                return;
            }

            AskService.ClearAllAsks();
            Utilities.SendStaffMessage(Color.White, "CLEAR: {{FFFFFF}}{0} telah menghapus semua ask di server.", player.CharInfo.Username);
        }
    }
}