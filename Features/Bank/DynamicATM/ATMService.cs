using ProjectSMP.Core;
using ProjectSMP.Plugins.ColAndreas;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using SampSharp.Streamer.Events;
using SampSharp.Streamer.World;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectSMP.Features.Bank.DynamicATM
{
    public static class ATMService
    {
        private const int MaxATMs = 100;
        private const int ATMModel = 19324;
        private const float PolygonRadius = 1.5f;
        private const string Table = "atm_locations";

        private static readonly Dictionary<int, DynamicATMData> ATMs = new();
        private static readonly Dictionary<int, int> _editingATM = new();

        public static void Initialize()
        {
            ATMGridManager.Initialize();
        }

        public static async Task<List<DynamicATMData>> LoadDataAsync()
        {
            var rows = await DatabaseManager.QueryAsync<ATMLocationRow>(
                $"SELECT ID, vw AS Vw, interior AS Interior, " +
                $"posx AS Posx, posy AS Posy, posz AS Posz, " +
                $"rotx AS Rotx, roty AS Roty, rotz AS Rotz FROM `{Table}`");

            var list = new List<DynamicATMData>();
            foreach (var r in rows)
            {
                list.Add(new DynamicATMData
                {
                    Id = r.ID,
                    VirtualWorld = r.Vw,
                    Interior = r.Interior,
                    PosX = r.Posx,
                    PosY = r.Posy,
                    PosZ = r.Posz,
                    RotX = r.Rotx,
                    RotY = r.Roty,
                    RotZ = r.Rotz
                });
            }
            return list;
        }

        public static void CreateObjects(List<DynamicATMData> list)
        {
            foreach (var data in list)
            {
                ATMs[data.Id] = data;
                Rebuild(data.Id);
            }
            Console.WriteLine($"[+] MariaDB - Load ATM Location data ({ATMs.Count} count).");
        }

        public static async Task<int> CreateAsync(Vector3 pos, Vector3 rot, int vw, int interior)
        {
            var id = GetFreeId();
            if (id == -1) return -1;

            var data = new DynamicATMData
            {
                Id = id,
                VirtualWorld = vw,
                Interior = interior,
                PosX = pos.X,
                PosY = pos.Y,
                PosZ = pos.Z,
                RotX = rot.X,
                RotY = rot.Y,
                RotZ = rot.Z
            };

            await DatabaseManager.ExecuteAsync(
                $"INSERT INTO `{Table}` (ID, vw, interior, posx, posy, posz, rotx, roty, rotz) " +
                "VALUES (@Id, @Vw, @Interior, @PosX, @PosY, @PosZ, @RotX, @RotY, @RotZ)",
                new { data.Id, Vw = data.VirtualWorld, data.Interior, data.PosX, data.PosY, data.PosZ, data.RotX, data.RotY, data.RotZ });

            ATMs[id] = data;
            Rebuild(id);
            return id;
        }

        public static async Task SaveAsync(int id)
        {
            if (!ATMs.TryGetValue(id, out var data)) return;

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET vw=@Vw, interior=@Interior, " +
                "posx=@PosX, posy=@PosY, posz=@PosZ, rotx=@RotX, roty=@RotY, rotz=@RotZ WHERE ID=@Id",
                new { Vw = data.VirtualWorld, data.Interior, data.PosX, data.PosY, data.PosZ, data.RotX, data.RotY, data.RotZ, data.Id });
        }

        public static async Task DeleteAsync(int id)
        {
            if (!ATMs.TryGetValue(id, out var data)) return;

            DestroyObjects(data);
            ATMs.Remove(id);

            await DatabaseManager.ExecuteAsync($"DELETE FROM `{Table}` WHERE ID=@Id", new { Id = id });
        }

        public static void Rebuild(int id)
        {
            if (!ATMs.TryGetValue(id, out var data)) return;

            DestroyObjects(data);

            var pos = new Vector3(data.PosX, data.PosY, data.PosZ);
            var rot = new Vector3(data.RotX, data.RotY, data.RotZ);

            data.ColDCIndex = ColAndreasDynamicObjectManager.CreateObject_DC(ATMModel, pos, rot, worldid: data.VirtualWorld, interiorid: data.Interior);

            var labelText = $"{{C6E2FF}}[ID: {data.Id}]\n{{FFFFFF}}Status: {{73d222}}Beroperasi\n{{FFFFFF}}Tekan '{{FF0000}}F{{FFFFFF}}' untuk berinteraksi";
            data.Label = new DynamicTextLabel(labelText, Color.White, pos + new Vector3(0, 0, 1.0f), 3.0f);
            data.Label.World = data.VirtualWorld;
            data.Label.Interior = data.Interior;

            data.Polygon = PolygonManager.CreateCircularPolygon(data.PosX, data.PosY, data.PosZ, PolygonRadius);
            ATMGridManager.Add(id, data.PosX, data.PosY);
        }

        public static int CheckPlayerInATM(Player player)
        {
            var pos = player.Position;
            var inCell = ATMGridManager.GetInCell(pos.X, pos.Y);

            foreach (var id in inCell)
            {
                if (!ATMs.TryGetValue(id, out var data)) continue;
                if (data.Interior != player.Interior || data.VirtualWorld != player.VirtualWorld) continue;
                if (data.Polygon != null && data.Polygon.IsPointInside(pos))
                    return id;
            }

            return -1;
        }

        public static void HandleInteract(Player player)
        {
            if (!player.IsCharLoaded) return;

            var atmId = CheckPlayerInATM(player);
            if (atmId == -1) return;

            ATMDialogManager.ShowATMInterface(player);
        }

        public static void StartEdit(Player player, int id)
        {
            if (!ATMs.TryGetValue(id, out var data)) return;

            var obj = ColAndreasDynamicObjectManager.GetDynamicObject(data.ColDCIndex);
            if (obj == null) return;

            _editingATM[player.Id] = id;

            EventHandler<PlayerEditEventArgs> handler = null;
            handler = (sender, e) =>
            {
                if (e.Player?.Id != player.Id) return;
                if (e.Response == EditObjectResponse.Final)
                {
                    obj.Edited -= handler;
                    data.PosX = e.Position.X;
                    data.PosY = e.Position.Y;
                    data.PosZ = e.Position.Z;
                    data.RotX = e.Rotation.X;
                    data.RotY = e.Rotation.Y;
                    data.RotZ = e.Rotation.Z;
                    _editingATM.Remove(player.Id);

                    _ = SaveAsync(id);
                    Rebuild(id);

                    player.SendClientMessage(Color.White,
                        $"{Msg.AdmCmd} Posisi ATM ID {id} berhasil diperbarui.");
                }
                else if (e.Response == EditObjectResponse.Cancel)
                {
                    obj.Edited -= handler;
                    obj.Position = new Vector3(data.PosX, data.PosY, data.PosZ);
                    obj.Rotation = new Vector3(data.RotX, data.RotY, data.RotZ);
                    _editingATM.Remove(player.Id);

                    player.SendClientMessage(Color.White,
                        $"{Msg.AdmCmd} Edit ATM dibatalkan.");
                }
            };

            obj.Edited += handler;
            obj.Edit(player);
        }

        public static DynamicATMData GetATM(int id) =>
            ATMs.TryGetValue(id, out var data) ? data : null;

        public static bool Exists(int id) => ATMs.ContainsKey(id);

        private static void DestroyObjects(DynamicATMData data)
        {
            if (data.ColDCIndex >= 0) { ColAndreasDynamicObjectManager.DestroyObject(data.ColDCIndex); data.ColDCIndex = -1; }
            data.Label?.Dispose();
            data.Polygon?.Clear();
            ATMGridManager.Remove(data.Id, data.PosX, data.PosY);
        }

        private static int GetFreeId()
        {
            for (var i = 0; i < MaxATMs; i++)
                if (!ATMs.ContainsKey(i)) return i;
            return -1;
        }
    }
}