using ProjectSMP.Entities.Players.Needs.Data;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Needs
{
    internal static class NeedsHudManager
    {
        private const int MaxHudComponents = 5;
        private const int MaxHuds = 5;
        private const float HudSpacing = 22.0f;

        private static readonly Dictionary<int, PlayerNeedsData> _playerHuds = new();

        public static void Initialize(Player player)
        {
            if (_playerHuds.ContainsKey(player.Id))
                Cleanup(player);

            _playerHuds[player.Id] = new PlayerNeedsData();
            GenerateHud(player);
            ShowHud(player);
        }

        public static void Cleanup(Player player)
        {
            if (!_playerHuds.TryGetValue(player.Id, out var data)) return;

            for (var i = 0; i < MaxHuds; i++)
            {
                for (var j = 0; j < MaxHudComponents; j++)
                {
                    data.HudComponents[j, i]?.Dispose();
                    data.HudComponents[j, i] = null;
                }
            }

            _playerHuds.Remove(player.Id);
        }

        public static void UpdateHud(Player player)
        {
            if (!_playerHuds.TryGetValue(player.Id, out var data)) return;

            for (var i = 0; i < data.ActiveHudCount; i++)
            {
                var barTd = data.HudComponents[(int)NeedsHudComponent.Bar, i];
                var textTd = data.HudComponents[(int)NeedsHudComponent.Text, i];

                if (barTd == null || textTd == null) continue;

                var value = GetHudValue(player, data.ActiveHudNames[i]);
                UpdateHudTextDraw(player, value, barTd, textTd);
            }
        }

        public static void RegenerateHud(Player player)
        {
            Cleanup(player);
            Initialize(player);
        }

        public static (int count, float[] coords) GetActiveHudInfo(Player player)
        {
            if (!_playerHuds.TryGetValue(player.Id, out var data))
                return (0, Array.Empty<float>());
            return (data.ActiveHudCount, data.ActiveHudCoords);
        }

        private static void GenerateHud(Player player)
        {
            var data = _playerHuds[player.Id];
            var activeCount = CalculateNeededHud(player, data);
            data.ActiveHudCount = activeCount;

            for (var i = 0; i < activeCount; i++)
            {
                var posX = 320.0f + (i * HudSpacing) - ((activeCount / 2.0f) * HudSpacing);
                data.ActiveHudCoords[i] = posX;

                switch (data.ActiveHudNames[i])
                {
                    case "Health":
                        CreateHealthHud(player, data, i, posX);
                        break;
                    case "Armour":
                        CreateArmourHud(player, data, i, posX);
                        break;
                    case "Hunger":
                        CreateHungerHud(player, data, i, posX);
                        break;
                    case "Thirst":
                        CreateThirstHud(player, data, i, posX);
                        break;
                    case "Stress":
                        CreateStressHud(player, data, i, posX);
                        break;
                }
            }
        }

        private static int CalculateNeededHud(Player player, PlayerNeedsData data)
        {
            var count = 0;
            var settings = new[] { player.Settings.ShowHealth, player.Settings.ShowArmour, player.Settings.ShowHunger, player.Settings.ShowThirst, player.Settings.ShowStress };
            var names = new[] { "Health", "Armour", "Hunger", "Thirst", "Stress" };

            for (var i = 0; i < settings.Length; i++)
            {
                data.ActiveHudNames[i] = "";
                data.ActiveHudCoords[i] = 0.0f;
            }

            for (var i = 0; i < settings.Length; i++)
            {
                if (settings[i])
                {
                    data.ActiveHudNames[count] = names[i];
                    count++;
                }
            }

            return count;
        }

        private static void CreateHealthHud(Player player, PlayerNeedsData data, int index, float posX)
        {
            CreateHudComponent(player, data, NeedsHudComponent.Background, index, posX, 424.0f, "_", TextDrawFont.Normal, 0.6f, 2.2f, 415.0f, 18.0f, TextDrawAlignment.Center, -1, 255, -314358960, true);
            CreateHudComponent(player, data, NeedsHudComponent.Box, index, posX, 427.0f, "_", TextDrawFont.Normal, 0.6f, 1.55f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, 1158943999, true);
            CreateHudComponent(player, data, NeedsHudComponent.Bar, index, posX, 445.0f, "_", TextDrawFont.Normal, 0.6f, -2.35f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, -297515777, true);
            CreateHudComponent(player, data, NeedsHudComponent.Icon, index, posX - 4.0f, 431.0f, "HUD:radar_girlfriend", TextDrawFont.DrawSprite, 0.6f, 2.0f, 8.0f, 7.0f, TextDrawAlignment.Center, -1, 255, 50, true);
            CreateHudComponent(player, data, NeedsHudComponent.Text, index, posX, 414.0f, "100", TextDrawFont.Slim, 0.133f, 0.699f, 400.0f, 17.0f, TextDrawAlignment.Center, -1, 255, 50, false);
        }

        private static void CreateArmourHud(Player player, PlayerNeedsData data, int index, float posX)
        {
            CreateHudComponent(player, data, NeedsHudComponent.Background, index, posX, 424.0f, "_", TextDrawFont.Normal, 0.6f, 2.2f, 415.0f, 18.0f, TextDrawAlignment.Center, -1, 255, -724314032, true);
            CreateHudComponent(player, data, NeedsHudComponent.Box, index, posX, 427.0f, "_", TextDrawFont.Normal, 0.6f, 1.55f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, 2088598017, true);
            CreateHudComponent(player, data, NeedsHudComponent.Bar, index, posX, 445.0f, "_", TextDrawFont.Normal, 0.6f, -2.35f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, -724313857, true);
            CreateHudComponent(player, data, NeedsHudComponent.Icon, index, posX - 4.0f, 431.0f, "HUD:radar_tshirt", TextDrawFont.DrawSprite, 0.6f, 2.0f, 8.0f, 7.0f, TextDrawAlignment.Center, -1, 255, 50, true);
            CreateHudComponent(player, data, NeedsHudComponent.Text, index, posX, 414.0f, "100", TextDrawFont.Slim, 0.133f, 0.699f, 400.0f, 17.0f, TextDrawAlignment.Center, -1, 255, 50, false);
        }

        private static void CreateHungerHud(Player player, PlayerNeedsData data, int index, float posX)
        {
            CreateHudComponent(player, data, NeedsHudComponent.Background, index, posX, 424.0f, "_", TextDrawFont.Normal, 0.6f, 2.2f, 415.0f, 18.0f, TextDrawAlignment.Center, -1, 255, -40369072, true);
            CreateHudComponent(player, data, NeedsHudComponent.Box, index, posX, 427.0f, "_", TextDrawFont.Normal, 0.6f, 1.55f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, -1856306945, true);
            CreateHudComponent(player, data, NeedsHudComponent.Bar, index, posX, 445.0f, "_", TextDrawFont.Normal, 0.6f, -2.35f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, -40368897, true);
            CreateHudComponent(player, data, NeedsHudComponent.Icon, index, posX - 4.0f, 431.0f, "HUD:radar_datefood", TextDrawFont.DrawSprite, 0.6f, 2.0f, 8.0f, 7.0f, TextDrawAlignment.Center, -1, 255, 50, true);
            CreateHudComponent(player, data, NeedsHudComponent.Text, index, posX, 414.0f, "100", TextDrawFont.Slim, 0.133f, 0.699f, 400.0f, 17.0f, TextDrawAlignment.Center, -1, 255, 50, false);
        }

        private static void CreateThirstHud(Player player, PlayerNeedsData data, int index, float posX)
        {
            CreateHudComponent(player, data, NeedsHudComponent.Background, index, posX, 424.0f, "_", TextDrawFont.Normal, 0.6f, 2.2f, 415.0f, 18.0f, TextDrawAlignment.Center, -1, 255, 79756112, true);
            CreateHudComponent(player, data, NeedsHudComponent.Box, index, posX, 427.0f, "_", TextDrawFont.Normal, 0.6f, 1.55f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, 6849279, true);
            CreateHudComponent(player, data, NeedsHudComponent.Bar, index, posX, 445.0f, "_", TextDrawFont.Normal, 0.6f, -2.35f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, 12973823, true);
            CreateHudComponent(player, data, NeedsHudComponent.Icon, index, posX - 4.0f, 431.0f, "HUD:radar_diner", TextDrawFont.DrawSprite, 0.6f, 2.0f, 8.0f, 7.0f, TextDrawAlignment.Center, -1, 255, 50, true);
            CreateHudComponent(player, data, NeedsHudComponent.Text, index, posX, 414.0f, "100", TextDrawFont.Slim, 0.133f, 0.699f, 400.0f, 17.0f, TextDrawAlignment.Center, -1, 255, 50, false);
        }

        private static void CreateStressHud(Player player, PlayerNeedsData data, int index, float posX)
        {
            CreateHudComponent(player, data, NeedsHudComponent.Background, index, posX, 424.0f, "_", TextDrawFont.Normal, 0.6f, 2.2f, 415.0f, 18.0f, TextDrawAlignment.Center, -1, 255, -314358960, true);
            CreateHudComponent(player, data, NeedsHudComponent.Box, index, posX, 427.0f, "_", TextDrawFont.Normal, 0.6f, 1.55f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, 1158943999, true);
            CreateHudComponent(player, data, NeedsHudComponent.Bar, index, posX, 445.0f, "_", TextDrawFont.Normal, 0.6f, -2.35f, 415.0f, 14.0f, TextDrawAlignment.Center, -1, 255, -297515777, true);
            CreateHudComponent(player, data, NeedsHudComponent.Icon, index, posX - 4.0f, 431.0f, "HUD:radar_school", TextDrawFont.DrawSprite, 0.6f, 2.0f, 8.0f, 7.0f, TextDrawAlignment.Center, -1, 255, 50, true);
            CreateHudComponent(player, data, NeedsHudComponent.Text, index, posX, 414.0f, "100", TextDrawFont.Slim, 0.133f, 0.699f, 400.0f, 17.0f, TextDrawAlignment.Center, -1, 255, 50, false);
        }

        private static void CreateHudComponent(Player player, PlayerNeedsData data, NeedsHudComponent component, int index, float posX, float posY, string text, TextDrawFont font, float sizeX, float sizeY, float sizeLX, float sizeLY, TextDrawAlignment align, int color, int bgColor, int boxColor, bool useBox)
        {
            var td = new PlayerTextDraw(player, new Vector2(posX, posY), text)
            {
                Font = font,
                LetterSize = new Vector2(sizeX, sizeY),
                Width = sizeLX,
                Height = sizeLY,
                Outline = 1,
                Shadow = 0,
                Alignment = align,
                ForeColor = new Color(color),
                BackColor = new Color(bgColor),
                BoxColor = new Color(boxColor),
                UseBox = useBox,
                Proportional = true,
                Selectable = false
            };

            data.HudComponents[(int)component, index] = td;
        }

        private static void UpdateHudTextDraw(Player player, float value, PlayerTextDraw barTd, PlayerTextDraw textTd)
        {
            var intValue = (int)value;
            textTd.Text = intValue.ToString();
            textTd.Show();

            var barSize = ConvertPercentageToRange(value);
            barTd.LetterSize = new Vector2(0.6f, barSize);
            barTd.Show();
        }

        private static float ConvertPercentageToRange(float percent)
        {
            if (percent < 0 || percent > 100) return -0.399f;
            return -0.399f + (percent / 100.0f) * (-2.349f - (-0.399f));
        }

        private static float GetHudValue(Player player, string hudName)
        {
            return hudName switch
            {
                "Health" => player.Vitals.Health,
                "Armour" => player.Vitals.Armour,
                "Hunger" => player.Vitals.Hunger,
                "Thirst" => player.Vitals.Energy,
                "Stress" => player.Vitals.Stress,
                _ => 0f
            };
        }

        private static void ShowHud(Player player)
        {
            if (!_playerHuds.TryGetValue(player.Id, out var data)) return;

            for (var i = 0; i < MaxHuds; i++)
            {
                for (var j = 0; j < MaxHudComponents; j++)
                {
                    data.HudComponents[j, i]?.Show();
                }
            }
        }

        public static void HideHud(Player player)
        {
            if (!_playerHuds.TryGetValue(player.Id, out var data)) return;

            for (var i = 0; i < MaxHuds; i++)
            {
                for (var j = 0; j < MaxHudComponents; j++)
                {
                    data.HudComponents[j, i]?.Hide();
                }
            }
        }
    }
}