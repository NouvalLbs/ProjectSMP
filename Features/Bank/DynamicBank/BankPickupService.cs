using ProjectSMP.Core;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.Streamer.World;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectSMP.Features.Bank.DynamicBank
{
    public static class BankPickupService
    {
        private const int MaxBanks = 100;
        private const int PickupModel = 1274;
        private const int PickupType = 23;
        private const float PolygonRadius = 1.5f;
        private const string Table = "bank_locations";

        private static readonly Dictionary<int, DynamicBankData> Banks = new();

        public static void Initialize()
        {
            BankPickupGridManager.Initialize();
        }

        public static async Task<List<DynamicBankData>> LoadDataAsync()
        {
            var rows = await DatabaseManager.QueryAsync<BankLocationRow>(
                $"SELECT ID, name AS Name, vw AS Vw, interior AS Interior, " +
                $"posx AS Posx, posy AS Posy, posz AS Posz FROM `{Table}`");

            var list = new List<DynamicBankData>();
            foreach (var r in rows)
            {
                list.Add(new DynamicBankData
                {
                    Id = r.ID,
                    Name = r.Name,
                    VirtualWorld = r.Vw,
                    Interior = r.Interior,
                    PosX = r.Posx,
                    PosY = r.Posy,
                    PosZ = r.Posz
                });
            }
            return list;
        }

        public static void CreateObjects(List<DynamicBankData> list)
        {
            foreach (var data in list)
            {
                Banks[data.Id] = data;
                Rebuild(data.Id);
            }
            Console.WriteLine($"[+] MariaDB - Load Bank Location data ({Banks.Count} count).");
        }

        public static async Task<int> CreateAsync(string name, Vector3 pos, int vw, int interior)
        {
            var id = GetFreeId();
            if (id == -1) return -1;

            var data = new DynamicBankData
            {
                Id = id,
                Name = name,
                VirtualWorld = vw,
                Interior = interior,
                PosX = pos.X,
                PosY = pos.Y,
                PosZ = pos.Z
            };

            await DatabaseManager.ExecuteAsync(
                $"INSERT INTO `{Table}` (ID, name, vw, interior, posx, posy, posz) " +
                "VALUES (@Id, @Name, @Vw, @Interior, @PosX, @PosY, @PosZ)",
                new { data.Id, data.Name, Vw = data.VirtualWorld, data.Interior, data.PosX, data.PosY, data.PosZ });

            Banks[id] = data;
            Rebuild(id);
            return id;
        }

        public static async Task SaveAsync(int id)
        {
            if (!Banks.TryGetValue(id, out var data)) return;

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET name=@Name, vw=@Vw, interior=@Interior, " +
                "posx=@PosX, posy=@PosY, posz=@PosZ WHERE ID=@Id",
                new { data.Name, Vw = data.VirtualWorld, data.Interior, data.PosX, data.PosY, data.PosZ, data.Id });
        }

        public static async Task DeleteAsync(int id)
        {
            if (!Banks.TryGetValue(id, out var data)) return;

            DestroyObjects(data);
            Banks.Remove(id);

            await DatabaseManager.ExecuteAsync($"DELETE FROM `{Table}` WHERE ID=@Id", new { Id = id });
        }

        public static void Rebuild(int id)
        {
            if (!Banks.TryGetValue(id, out var data)) return;

            DestroyObjects(data);

            var pos = new Vector3(data.PosX, data.PosY, data.PosZ);
            data.Pickup = new DynamicPickup(PickupModel, PickupType, pos, data.VirtualWorld, data.Interior);

            var labelText = $"{{FFFF00}}{data.Name}\n{{FFFFFF}}Tekan '{{FF0000}}F{{FFFFFF}}' untuk mengakses bank";
            data.Label = new DynamicTextLabel(labelText, Color.White, pos + new Vector3(0, 0, 0.5f), 5.0f, null, streamdistance: 5.0f);
            data.Label.World = data.VirtualWorld;
            data.Label.Interior = data.Interior;

            data.Polygon = PolygonManager.CreateCircularPolygon(data.PosX, data.PosY, data.PosZ, PolygonRadius);
            BankPickupGridManager.Add(id, data.PosX, data.PosY);
        }

        public static int CheckPlayerInBank(Player player)
        {
            var pos = player.Position;
            var inCell = BankPickupGridManager.GetInCell(pos.X, pos.Y);

            foreach (var id in inCell)
            {
                if (!Banks.TryGetValue(id, out var data)) continue;
                if (data.Interior != player.Interior || data.VirtualWorld != player.VirtualWorld) continue;
                if (data.Polygon != null && data.Polygon.IsPointInside(pos))
                    return id;
            }

            return -1;
        }

        public static void HandleInteract(Player player)
        {
            if (!player.IsCharLoaded) return;

            var bankId = CheckPlayerInBank(player);
            if (bankId == -1) return;

            BankDialogManager.ShowBankInterface(player);
        }

        public static DynamicBankData GetBank(int id) =>
            Banks.TryGetValue(id, out var data) ? data : null;

        public static bool Exists(int id) => Banks.ContainsKey(id);

        private static void DestroyObjects(DynamicBankData data)
        {
            data.Pickup?.Dispose();
            data.Label?.Dispose();
            data.Polygon?.Clear();
            BankPickupGridManager.Remove(data.Id, data.PosX, data.PosY);
        }

        private static int GetFreeId()
        {
            for (var i = 0; i < MaxBanks; i++)
                if (!Banks.ContainsKey(i)) return i;
            return -1;
        }
    }
}