using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Condition
{
    internal static class DeathScreenManager
    {
        private const int MaxDeathScreenComponents = 12;
        private static readonly Dictionary<int, PlayerTextDraw[]> _deathScreens = new();

        public static void Create(Player player)
        {
            Destroy(player);

            var tds = new PlayerTextDraw[MaxDeathScreenComponents];

            tds[0] = CreateTimerBar(player, 312.0f, 445.0f, 858993663);
            tds[1] = CreateTimerBar(player, 298.0f, 445.0f, 858993663);
            tds[2] = CreateTimerBar(player, 328.5f, 445.0f, 858993663);
            tds[3] = CreateTimerBar(player, 342.5f, 445.0f, 858993663);

            tds[4] = CreateText(player, 320.0f, 426.0f, ":", TextDrawFont.Slim, 0.133331f, 1.599997f);

            tds[5] = CreateText(player, 298.0f, 427.0f, "0", TextDrawFont.Slim, 0.179164f, 1.599997f);
            tds[6] = CreateText(player, 312.5f, 427.0f, "0", TextDrawFont.Slim, 0.179164f, 1.599997f);
            tds[7] = CreateText(player, 328.5f, 427.0f, "0", TextDrawFont.Slim, 0.179164f, 1.599997f);
            tds[8] = CreateText(player, 342.5f, 427.0f, "0", TextDrawFont.Slim, 0.179164f, 1.599997f);

            tds[9] = CreateText(player, 321.0f, 415.0f, "Bleeding_out", TextDrawFont.Slim, 0.133331f, 0.699998f);
            tds[10] = CreateText(player, 321.0f, 406.0f, "/death_belum_tersedia", TextDrawFont.Slim, 0.133331f, 0.699998f);
            tds[11] = CreateMainText(player, 321.0f, 389.0f, "KAMU_PINGSAN_DAN_MENGALAMI_PENDARAHAN");

            _deathScreens[player.Id] = tds;

            foreach (var td in tds)
                td?.Show();
        }

        public static void Destroy(Player player)
        {
            if (!_deathScreens.TryGetValue(player.Id, out var tds)) return;

            foreach (var td in tds)
                td?.Dispose();

            _deathScreens.Remove(player.Id);
        }

        public static void UpdateTimer(Player player, int minutes, int seconds)
        {
            if (!_deathScreens.TryGetValue(player.Id, out var tds)) return;

            var minSt = (minutes / 10).ToString();
            var minNd = (minutes % 10).ToString();
            var secSt = (seconds / 10).ToString();
            var secNd = (seconds % 10).ToString();

            if (tds[5] != null) tds[5].Text = minSt;
            if (tds[6] != null) tds[6].Text = minNd;
            if (tds[7] != null) tds[7].Text = secSt;
            if (tds[8] != null) tds[8].Text = secNd;

            foreach (var td in tds)
                td?.Show();
        }

        public static void UpdateStatus(Player player, int stage)
        {
            if (!_deathScreens.TryGetValue(player.Id, out var tds)) return;

            if (stage == 2)
            {
                if (tds[9] != null) tds[9].Text = "Respawn_in";
                if (tds[10] != null) tds[10].Text = "/death_untuk_respawn";
                if (tds[11] != null) tds[11].Text = "KAMU_PINGSAN_DAN_SEKARAT";
            }
            else
            {
                if (tds[9] != null) tds[9].Text = "Bleeding_out";
                if (tds[10] != null) tds[10].Text = "/death_belum_tersedia";
                if (tds[11] != null) tds[11].Text = "KAMU_PINGSAN_DAN_MENGALAMI_PENDARAHAN";
            }

            foreach (var td in tds)
                td?.Show();
        }

        private static PlayerTextDraw CreateTimerBar(Player player, float x, float y, int boxColor)
        {
            var td = new PlayerTextDraw(player, new Vector2(x, y), "_")
            {
                Font = TextDrawFont.Normal,
                LetterSize = new Vector2(0.6f, -2.349996f),
                Width = 415.0f,
                Height = 10.0f,
                Outline = 1,
                Shadow = 0,
                Alignment = TextDrawAlignment.Center,
                ForeColor = Color.White,
                BackColor = new Color(255),
                BoxColor = new Color(boxColor),
                UseBox = true,
                Proportional = true,
                Selectable = false
            };
            return td;
        }

        private static PlayerTextDraw CreateText(Player player, float x, float y, string text, TextDrawFont font, float letterX, float letterY)
        {
            var td = new PlayerTextDraw(player, new Vector2(x, y), text)
            {
                Font = font,
                LetterSize = new Vector2(letterX, letterY),
                Width = 400.0f,
                Height = 17.0f,
                Outline = font == TextDrawFont.Slim ? 0 : 1,
                Shadow = 0,
                Alignment = TextDrawAlignment.Center,
                ForeColor = Color.White,
                BackColor = new Color(255),
                BoxColor = new Color(50),
                UseBox = false,
                Proportional = true,
                Selectable = false
            };
            return td;
        }

        private static PlayerTextDraw CreateMainText(Player player, float x, float y, string text)
        {
            var td = new PlayerTextDraw(player, new Vector2(x, y), text)
            {
                Font = TextDrawFont.Pricedown,
                LetterSize = new Vector2(0.312497f, 1.549997f),
                Width = 400.0f,
                Height = 17.0f,
                Outline = 1,
                Shadow = 0,
                Alignment = TextDrawAlignment.Center,
                ForeColor = new Color(-651153409),
                BackColor = new Color(255),
                BoxColor = new Color(50),
                UseBox = false,
                Proportional = true,
                Selectable = false
            };
            return td;
        }
    }
}