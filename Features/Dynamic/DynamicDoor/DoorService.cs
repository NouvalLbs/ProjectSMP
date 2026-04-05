using ProjectSMP.Core;
using ProjectSMP.Extensions;
using ProjectSMP.Features.EnterExit;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.Streamer.World;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectSMP.Features.Dynamic.DynamicDoor
{
    public static class DoorService
    {
        private const int MaxDoors = 500;
        private const string Table = "doors";

        private static readonly Dictionary<int, DynamicDoorData> Doors = new();
        private static readonly Dictionary<int, bool> PlayerIsOutside = new();

        public static void Initialize()
        {
            DoorGridManager.Initialize();
        }

        public static async Task<List<DynamicDoorData>> LoadDataAsync()
        {
            var rows = await DatabaseManager.QueryAsync<DoorDatabaseRow>(
                $"SELECT ID, name AS Name, password AS Password, icon AS Icon, locked AS Locked, " +
                $"admin AS Admin, vip AS Vip, faction AS Faction, family AS Family, " +
                $"garage AS Garage, custom AS Custom, extvw AS Extvw, extint AS Extint, " +
                $"extposx AS Extposx, extposy AS Extposy, extposz AS Extposz, extposa AS Extposa, " +
                $"intvw AS Intvw, intint AS Intint, intposx AS Intposx, intposy AS Intposy, " +
                $"intposz AS Intposz, intposa AS Intposa, mapicon AS Mapicon FROM `{Table}`");

            var dataList = new List<DynamicDoorData>();

            foreach (var row in rows)
            {
                var data = new DynamicDoorData
                {
                    Id = row.ID,
                    MapIconId = row.Mapicon,
                    Name = row.Name,
                    Password = row.Password,
                    Icon = row.Icon,
                    Locked = row.Locked == 1,
                    AdminLevel = row.Admin,
                    VipLevel = row.Vip,
                    FactionId = row.Faction,
                    FamilyId = row.Family,
                    IsGarage = row.Garage == 1,
                    CustomInterior = row.Custom == 1,
                    ExtVirtualWorld = row.Extvw,
                    ExtInterior = row.Extint,
                    ExtPosX = row.Extposx,
                    ExtPosY = row.Extposy,
                    ExtPosZ = row.Extposz,
                    ExtAngle = row.Extposa,
                    IntVirtualWorld = row.Intvw,
                    IntInterior = row.Intint,
                    IntPosX = row.Intposx,
                    IntPosY = row.Intposy,
                    IntPosZ = row.Intposz,
                    IntAngle = row.Intposa
                };

                dataList.Add(data);
            }

            return dataList;
        }

        public static void CreateDoorObjects(List<DynamicDoorData> dataList)
        {
            foreach (var data in dataList)
            {
                Doors[data.Id] = data;
                UpdateDoor(data.Id);
            }

            Console.WriteLine($"[+] MariaDB - Load Door data ({Doors.Count} count).");
        }

        public static async Task<int> CreateAsync(string name, Vector3 position, float angle, int virtualWorld, int interior)
        {
            var doorId = GetFreeId();
            if (doorId == -1) return -1;

            var data = new DynamicDoorData
            {
                Id = doorId,
                MapIconId = -1,
                Name = name,
                ExtPosX = position.X,
                ExtPosY = position.Y,
                ExtPosZ = position.Z,
                ExtAngle = angle,
                ExtVirtualWorld = virtualWorld,
                ExtInterior = interior
            };

            await DatabaseManager.ExecuteAsync(
                $"INSERT INTO `{Table}` (ID, mapicon, name, extvw, extint, extposx, extposy, extposz, extposa) " +
                "VALUES (@Id, @MapIconId, @Name, @Extvw, @Extint, @ExtPosX, @ExtPosY, @ExtPosZ, @ExtAngle)",
                new
                {
                    data.Id,
                    data.MapIconId,
                    data.Name,
                    Extvw = data.ExtVirtualWorld,
                    Extint = data.ExtInterior,
                    data.ExtPosX,
                    data.ExtPosY,
                    data.ExtPosZ,
                    data.ExtAngle,
                });

            Doors[doorId] = data;
            UpdateDoor(doorId);

            return doorId;
        }

        public static async Task SaveAsync(int doorId)
        {
            if (!Doors.TryGetValue(doorId, out var data)) return;

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET name=@Name, password=@Password, icon=@Icon, locked=@Locked, " +
                "admin=@Admin, vip=@Vip, faction=@Faction, family=@Family, garage=@Garage, custom=@Custom, " +
                "extvw=@Extvw, extint=@Extint, extposx=@ExtPosX, extposy=@ExtPosY, extposz=@ExtPosZ, extposa=@ExtAngle, " +
                "intvw=@Intvw, intint=@Intint, intposx=@IntPosX, intposy=@IntPosY, intposz=@IntPosZ, intposa=@IntAngle, mapicon=@MapIconId " +
                "WHERE ID=@Id",
                new
                {
                    data.Name,
                    data.Password,
                    data.Icon,
                    Locked = data.Locked ? 1 : 0,
                    Admin = data.AdminLevel,
                    Vip = data.VipLevel,
                    Faction = data.FactionId,
                    Family = data.FamilyId,
                    Garage = data.IsGarage ? 1 : 0,
                    Custom = data.CustomInterior ? 1 : 0,
                    Extvw = data.ExtVirtualWorld,
                    Extint = data.ExtInterior,
                    data.ExtPosX,
                    data.ExtPosY,
                    data.ExtPosZ,
                    data.ExtAngle,
                    Intvw = data.IntVirtualWorld,
                    Intint = data.IntInterior,
                    data.IntPosX,
                    data.IntPosY,
                    data.IntPosZ,
                    data.IntAngle,
                    data.MapIconId,
                    data.Id
                });
        }

        public static async Task DeleteAsync(int doorId)
        {
            if (!Doors.TryGetValue(doorId, out var data)) return;

            DestroyDoorObjects(data);
            Doors.Remove(doorId);

            await DatabaseManager.ExecuteAsync($"DELETE FROM `{Table}` WHERE ID=@Id", new { Id = doorId });
        }

        public static void UpdateDoor(int doorId)
        {
            if (!Doors.TryGetValue(doorId, out var data)) return;

            DestroyDoorObjects(data);

            var extPosition = new Vector3(data.ExtPosX, data.ExtPosY, data.ExtPosZ);
            data.ExtPickup = new DynamicPickup(data.Icon, 23, extPosition, data.ExtVirtualWorld, data.ExtInterior);

            var keyText = data.IsGarage ? "ALT" : "ENTER";
            var extLabelText = $"{{00FFFF}}[ID: {data.Id}]\n{{FFFF00}}{data.Name}\n{{FFFFFF}}Press '{{FF0000}}{keyText}{{FFFFFF}}' to enter/exit the door";
            data.ExtLabel = new DynamicTextLabel(extLabelText, Color.Yellow, extPosition + new Vector3(0, 0, 0.35f), 5.0f, null, streamdistance: 5.0f);

            if (data.MapIconId != -1)
            {
                data.ExtMapIcon = new DynamicMapIcon(
                    extPosition,
                    data.MapIconId,
                    MapIconType.Global,
                    data.ExtVirtualWorld,
                    data.ExtInterior,
                    null,
                    150.0f
                );
            }

            data.ExtLabel.World = data.ExtVirtualWorld;
            data.ExtLabel.Interior = data.ExtInterior;

            data.ExtPolygon = PolygonManager.CreateCircularPolygon(data.ExtPosX, data.ExtPosY, data.ExtPosZ, 1.5f);
            DoorGridManager.AddDoor(doorId, data.ExtPosX, data.ExtPosY);

            if (data.IntPosX != 0.0f && data.IntPosY != 0.0f && data.IntPosZ != 0.0f)
            {
                var intPosition = new Vector3(data.IntPosX, data.IntPosY, data.IntPosZ);
                data.IntPickup = new DynamicPickup(data.Icon, 23, intPosition, data.IntVirtualWorld, data.IntInterior);

                var intLabelText = $"{{00FFFF}}[ID: {data.Id}]\n{{FFFF00}}{data.Name}\n{{FFFFFF}}Press '{{FF0000}}{keyText}{{FFFFFF}}' to enter/exit the door";
                data.IntLabel = new DynamicTextLabel(intLabelText, Color.Yellow, intPosition + new Vector3(0, 0, 0.7f), 5.0f, null, streamdistance: 7.0f);
                data.IntLabel.World = data.IntVirtualWorld;
                data.IntLabel.Interior = data.IntInterior;

                data.IntPolygon = PolygonManager.CreateCircularPolygon(data.IntPosX, data.IntPosY, data.IntPosZ, 1.5f);
                DoorGridManager.AddDoor(doorId, data.IntPosX, data.IntPosY);
            }
        }

        public static int CheckPlayerInDoor(Player player, out bool isOutside)
        {
            var position = player.Position;
            var doorsInCell = DoorGridManager.GetDoorsInCell(position.X, position.Y);

            foreach (var doorId in doorsInCell)
            {
                if (!Doors.TryGetValue(doorId, out var data)) continue;

                if (data.ExtPolygon != null && data.ExtPolygon.IsPointInside(position))
                {
                    if (data.ExtInterior == player.Interior && data.ExtVirtualWorld == player.VirtualWorld)
                    {
                        isOutside = true;
                        return doorId;
                    }
                }

                if (data.IntPolygon != null && data.IntPolygon.IsPointInside(position))
                {
                    if (data.IntInterior == player.Interior && data.IntVirtualWorld == player.VirtualWorld)
                    {
                        isOutside = false;
                        return doorId;
                    }
                }
            }

            isOutside = false;
            return -1;
        }

        public static bool CanEnterDoor(Player player, int doorId)
        {
            if (!Doors.TryGetValue(doorId, out var data)) return false;

            if (data.IntPosX == 0.0f && data.IntPosY == 0.0f && data.IntPosZ == 0.0f)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Pintu masuk ini tidak bisa dimasuki masih dalam pembangunan.");
                return false;
            }

            if (data.Locked)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Pintu masuk ini sedang terkunci saat ini.");
                return false;
            }

            if (data.AdminLevel > player.Admin)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Level admin Anda tidak cukup untuk memasuki pintu ini.");
                return false;
            }

            return true;
        }

        public static void ToggleDoor(Player player, int doorId, bool isOutside, string password = "")
        {
            if (!Doors.TryGetValue(doorId, out var data)) return;

            if (isOutside)
            {
                if (!CanEnterDoor(player, doorId)) return;

                if (!string.IsNullOrEmpty(data.Password))
                {
                    if (string.IsNullOrEmpty(password))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /enter [Password]");
                        return;
                    }

                    if (!data.Password.Equals(password, StringComparison.Ordinal))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Password pintu tidak valid.");
                        return;
                    }
                }

                player.SetPositionSafe(new Vector3(data.IntPosX, data.IntPosY, data.IntPosZ));
                player.Angle = data.IntAngle;
                player.SetInteriorSafe(data.IntInterior);
                player.SetVirtualWorldSafe(data.IntVirtualWorld);
                player.SetWeather(0);
            }
            else
            {
                player.SetPositionSafe(new Vector3(data.ExtPosX, data.ExtPosY, data.ExtPosZ));
                player.Angle = data.ExtAngle;
                player.SetInteriorSafe(data.ExtInterior);
                player.SetVirtualWorldSafe(data.ExtVirtualWorld);
            }

            player.PutCameraBehindPlayer();
            EnterExitService.ProcessEnterExit(player, () =>
            {
                if (!player.IsDisposed)
                    player.ToggleControllableSafe(true);
            });
        }

        public static DynamicDoorData GetDoor(int doorId)
        {
            return Doors.TryGetValue(doorId, out var data) ? data : null;
        }

        public static bool Exists(int doorId)
        {
            return Doors.ContainsKey(doorId);
        }

        private static void DestroyDoorObjects(DynamicDoorData data)
        {
            data.ExtPickup?.Dispose();
            data.ExtLabel?.Dispose();
            data.ExtMapIcon?.Dispose();
            data.ExtMapIcon = null;
            data.ExtPolygon?.Clear();
            DoorGridManager.RemoveDoor(data.Id, data.ExtPosX, data.ExtPosY);

            data.IntPickup?.Dispose();
            data.IntLabel?.Dispose();
            data.IntPolygon?.Clear();
            if (data.IntPosX != 0.0f && data.IntPosY != 0.0f)
            {
                DoorGridManager.RemoveDoor(data.Id, data.IntPosX, data.IntPosY);
            }
        }

        private static int GetFreeId()
        {
            for (int i = 0; i < MaxDoors; i++)
            {
                if (!Doors.ContainsKey(i))
                    return i;
            }
            return -1;
        }

        public static void HandleDoorKeyPress(Player player)
        {
            if (!player.IsCharLoaded)
                return;

            if ((DateTime.Now - player.LastDoorInteraction).TotalSeconds < 3.0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu harus menunggu beberapa detik sebelum menggunakan pintu lagi.");
                return;
            }
            player.LastDoorInteraction = DateTime.Now;

            var doorId = CheckPlayerInDoor(player, out bool isOutside);
            if (doorId == -1)
                return;

            if (!Doors.TryGetValue(doorId, out var data))
                return;

            if (!isOutside)
            {
                ToggleDoor(player, doorId, isOutside);
                return;
            }

            if (!CanEnterDoor(player, doorId))
                return;

            if (!string.IsNullOrEmpty(data.Password))
            {
                ShowPasswordDialog(player, doorId);
                return;
            }

            ToggleDoor(player, doorId, isOutside);
        }

        private static void ShowPasswordDialog(Player player, int doorId)
        {
            player.ShowInput(
                "Door Password",
                "{FFFFFF}Pintu ini memerlukan password untuk masuk.\n{FFFF00}Masukkan password pintu:")
                .WithButtons("Enter", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                        return;

                    var password = e.InputText;
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Password tidak boleh kosong!");
                        ShowPasswordDialog(player, doorId);
                        return;
                    }

                    var currentDoorId = CheckPlayerInDoor(player, out bool isOutside);
                    if (currentDoorId != doorId || !isOutside)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Kamu sudah tidak berada di area pintu!");
                        return;
                    }

                    if (!Doors.TryGetValue(doorId, out var data))
                        return;

                    if (!data.Password.Equals(password, StringComparison.Ordinal))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Password pintu tidak valid.");
                        return;
                    }

                    ToggleDoor(player, doorId, isOutside);
                });
        }
    }
}