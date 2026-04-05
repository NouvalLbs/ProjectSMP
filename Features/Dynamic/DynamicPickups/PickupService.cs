using ProjectSMP.Core;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.Streamer.World;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectSMP.Features.Dynamic.DynamicPickups
{
    public static class PickupService
    {
        private const int MaxPickups = 500;
        private const string Table = "pickups";

        private static readonly Dictionary<int, DynamicPickupData> Pickups = new();

        public static void Initialize()
        {
            PickupGridManager.Initialize();
        }

        public static async Task<List<DynamicPickupData>> LoadDataAsync()
        {
            var rows = await DatabaseManager.QueryAsync<PickupDatabaseRow>(
                $"SELECT ID, title AS Title, name AS Name, model AS Model, type AS Type, " +
                $"vw AS Vw, interior AS Interior, posx AS Posx, posy AS Posy, posz AS Posz, " +
                $"callback AS Callback FROM `{Table}`");

            var dataList = new List<DynamicPickupData>();

            foreach (var row in rows)
            {
                var data = new DynamicPickupData
                {
                    Id = row.ID,
                    Title = row.Title,
                    Name = row.Name,
                    ModelId = row.Model,
                    Type = row.Type,
                    VirtualWorld = row.Vw,
                    Interior = row.Interior,
                    PosX = row.Posx,
                    PosY = row.Posy,
                    PosZ = row.Posz,
                    Callback = row.Callback
                };

                dataList.Add(data);
            }

            return dataList;
        }

        public static void CreatePickupObjects(List<DynamicPickupData> dataList)
        {
            foreach (var data in dataList)
            {
                Pickups[data.Id] = data;
                UpdatePickup(data.Id);
            }

            Console.WriteLine($"[+] MariaDB - Load Pickup data ({Pickups.Count} count).");
        }

        public static async Task<int> CreateAsync(string title, string name, int modelId, int type, string callback, Vector3 position, int virtualWorld, int interior)
        {
            var pickupId = GetFreeId();
            if (pickupId == -1) return -1;

            var data = new DynamicPickupData
            {
                Id = pickupId,
                Title = title,
                Name = name,
                ModelId = modelId,
                Type = type,
                Callback = callback,
                PosX = position.X,
                PosY = position.Y,
                PosZ = position.Z,
                VirtualWorld = virtualWorld,
                Interior = interior
            };

            await DatabaseManager.ExecuteAsync(
                $"INSERT INTO `{Table}` (ID, title, name, model, type, callback, vw, interior, posx, posy, posz) " +
                "VALUES (@Id, @Title, @Name, @Model, @Type, @Callback, @Vw, @Interior, @PosX, @PosY, @PosZ)",
                new
                {
                    data.Id,
                    data.Title,
                    data.Name,
                    Model = data.ModelId,
                    data.Type,
                    data.Callback,
                    Vw = data.VirtualWorld,
                    data.Interior,
                    data.PosX,
                    data.PosY,
                    data.PosZ
                });

            Pickups[pickupId] = data;
            UpdatePickup(pickupId);

            return pickupId;
        }

        public static async Task SaveAsync(int pickupId)
        {
            if (!Pickups.TryGetValue(pickupId, out var data)) return;

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET title=@Title, name=@Name, model=@Model, type=@Type, " +
                "callback=@Callback, vw=@Vw, interior=@Interior, posx=@PosX, posy=@PosY, posz=@PosZ WHERE ID=@Id",
                new
                {
                    data.Title,
                    data.Name,
                    Model = data.ModelId,
                    data.Type,
                    data.Callback,
                    Vw = data.VirtualWorld,
                    data.Interior,
                    data.PosX,
                    data.PosY,
                    data.PosZ,
                    data.Id
                });
        }

        public static async Task DeleteAsync(int pickupId)
        {
            if (!Pickups.TryGetValue(pickupId, out var data)) return;

            DestroyPickupObjects(data);
            Pickups.Remove(pickupId);

            await DatabaseManager.ExecuteAsync($"DELETE FROM `{Table}` WHERE ID=@Id", new { Id = pickupId });
        }

        public static void UpdatePickup(int pickupId)
        {
            if (!Pickups.TryGetValue(pickupId, out var data)) return;

            DestroyPickupObjects(data);

            var position = new Vector3(data.PosX, data.PosY, data.PosZ);
            data.Pickup = new DynamicPickup(data.ModelId, data.Type, position, data.VirtualWorld, data.Interior);

            var labelText = $"{data.Title}\n{data.Name}";
            data.Label = new DynamicTextLabel(labelText, Color.White, position, 3.0f, null, streamdistance: 3.0f);
            data.Label.World = data.VirtualWorld;
            data.Label.Interior = data.Interior;

            data.Polygon = PolygonManager.CreateCircularPolygon(data.PosX, data.PosY, data.PosZ, 1.5f);

            PickupGridManager.AddPickup(pickupId, data.PosX, data.PosY);
        }

        public static int CheckPlayerInPickup(Player player)
        {
            var position = player.Position;
            var pickupsInCell = PickupGridManager.GetPickupsInCell(position.X, position.Y);

            foreach (var pickupId in pickupsInCell)
            {
                if (!Pickups.TryGetValue(pickupId, out var data)) continue;

                if (data.Interior != player.Interior || data.VirtualWorld != player.VirtualWorld)
                    continue;

                if (data.Polygon != null && data.Polygon.IsPointInside(position))
                {
                    return pickupId;
                }
            }

            return -1;
        }

        public static int GetPickupByCallback(Player player, string callbackName)
        {
            var position = player.Position;
            var pickupsInCell = PickupGridManager.GetPickupsInCell(position.X, position.Y);

            foreach (var pickupId in pickupsInCell)
            {
                if (!Pickups.TryGetValue(pickupId, out var data)) continue;

                if (data.Interior != player.Interior || data.VirtualWorld != player.VirtualWorld)
                    continue;

                if (string.IsNullOrEmpty(data.Callback) || !data.Callback.Equals(callbackName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (data.Polygon != null && data.Polygon.IsPointInside(position))
                {
                    return pickupId;
                }
            }

            return -1;
        }

        public static DynamicPickupData GetPickup(int pickupId)
        {
            return Pickups.TryGetValue(pickupId, out var data) ? data : null;
        }

        public static bool Exists(int pickupId)
        {
            return Pickups.ContainsKey(pickupId);
        }

        private static void DestroyPickupObjects(DynamicPickupData data)
        {
            data.Pickup?.Dispose();
            data.Label?.Dispose();
            data.Polygon?.Clear();
            PickupGridManager.RemovePickup(data.Id, data.PosX, data.PosY);
        }

        private static int GetFreeId()
        {
            for (int i = 0; i < MaxPickups; i++)
            {
                if (!Pickups.ContainsKey(i))
                    return i;
            }
            return -1;
        }
    }
}