using ProjectSMP.Core;
using ProjectSMP.Plugins.ColAndreas;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using SampSharp.Streamer.Events;
using SampSharp.Streamer.World;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectSMP.Features.Jobs.Side.Trashmaster.DynamicTrash
{
    public static class TrashService
    {
        private const int MaxTrashes = 200;
        private const float PolygonRadius = 1.5f;
        private const string Table = "trash_locations";
        private const int FillIntervalMs = 10 * 60 * 1000;
        private const int MaxAmount = 100;

        private static readonly int[] TrashModels = { 1344, 1236 };
        private static readonly Random Rng = new();
        private static readonly Dictionary<int, DynamicTrashData> Trashes = new();
        private static readonly Dictionary<int, int> _editingTrash = new();
        private static Timer _fillTimer;

        public static void Initialize()
        {
            TrashGridManager.Initialize();

            _fillTimer = new Timer(FillIntervalMs, true);
            _fillTimer.Tick += OnFillTick;
        }

        public static void Dispose()
        {
            _fillTimer?.Dispose();
        }

        private static void OnFillTick(object sender, EventArgs e)
        {
            var dirty = new List<int>();

            foreach (var (id, data) in Trashes)
            {
                if (data.Amount >= MaxAmount) continue;
                data.Amount = Math.Min(MaxAmount, data.Amount + Rng.Next(5, 16));
                UpdateLabel(data);
                dirty.Add(id);
            }

            if (dirty.Count == 0) return;

            _ = Task.Run(async () =>
            {
                foreach (var id in dirty)
                {
                    if (Trashes.TryGetValue(id, out var d))
                        await SaveAmountAsync(id, d.Amount);
                }
            });
        }

        public static async Task<List<DynamicTrashData>> LoadDataAsync()
        {
            var rows = await DatabaseManager.QueryAsync<TrashLocationRow>(
                $"SELECT ID, vw AS Vw, interior AS Interior, " +
                $"posx AS Posx, posy AS Posy, posz AS Posz, " +
                $"rotx AS Rotx, roty AS Roty, rotz AS Rotz, " +
                $"model AS Model, amount AS Amount FROM `{Table}`");

            var list = new List<DynamicTrashData>();
            foreach (var r in rows)
            {
                list.Add(new DynamicTrashData
                {
                    Id = r.ID,
                    VirtualWorld = r.Vw,
                    Interior = r.Interior,
                    PosX = r.Posx,
                    PosY = r.Posy,
                    PosZ = r.Posz,
                    RotX = r.Rotx,
                    RotY = r.Roty,
                    RotZ = r.Rotz,
                    Model = r.Model,
                    Amount = r.Amount
                });
            }
            return list;
        }

        public static void CreateObjects(List<DynamicTrashData> list)
        {
            foreach (var data in list)
            {
                Trashes[data.Id] = data;
                Rebuild(data.Id);
            }
            Console.WriteLine($"[+] MariaDB - Load Trash Location data ({Trashes.Count} count).");
        }

        public static async Task<int> CreateAsync(Vector3 pos, Vector3 rot, int vw, int interior)
        {
            var id = GetFreeId();
            if (id == -1) return -1;

            var model = TrashModels[Rng.Next(TrashModels.Length)];

            var data = new DynamicTrashData
            {
                Id = id,
                VirtualWorld = vw,
                Interior = interior,
                PosX = pos.X,
                PosY = pos.Y,
                PosZ = pos.Z,
                RotX = rot.X,
                RotY = rot.Y,
                RotZ = rot.Z,
                Model = model,
                Amount = 0
            };

            await DatabaseManager.ExecuteAsync(
                $"INSERT INTO `{Table}` (ID, vw, interior, posx, posy, posz, rotx, roty, rotz, model, amount) " +
                "VALUES (@Id, @Vw, @Interior, @PosX, @PosY, @PosZ, @RotX, @RotY, @RotZ, @Model, @Amount)",
                new { data.Id, Vw = data.VirtualWorld, data.Interior, data.PosX, data.PosY, data.PosZ, data.RotX, data.RotY, data.RotZ, data.Model, data.Amount });

            Trashes[id] = data;
            Rebuild(id);
            return id;
        }

        public static async Task SaveAsync(int id)
        {
            if (!Trashes.TryGetValue(id, out var data)) return;

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET vw=@Vw, interior=@Interior, " +
                "posx=@PosX, posy=@PosY, posz=@PosZ, rotx=@RotX, roty=@RotY, rotz=@RotZ, model=@Model, amount=@Amount WHERE ID=@Id",
                new { Vw = data.VirtualWorld, data.Interior, data.PosX, data.PosY, data.PosZ, data.RotX, data.RotY, data.RotZ, data.Model, data.Amount, data.Id });
        }

        public static async Task SaveAmountAsync(int id, int amount)
        {
            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET amount=@Amount WHERE ID=@Id",
                new { Amount = amount, Id = id });
        }

        public static async Task DeleteAsync(int id)
        {
            if (!Trashes.TryGetValue(id, out var data)) return;

            DestroyObjects(data);
            Trashes.Remove(id);

            await DatabaseManager.ExecuteAsync($"DELETE FROM `{Table}` WHERE ID=@Id", new { Id = id });
        }

        public static void Rebuild(int id)
        {
            if (!Trashes.TryGetValue(id, out var data)) return;

            DestroyObjects(data);

            var pos = new Vector3(data.PosX, data.PosY, data.PosZ);
            var rot = new Vector3(data.RotX, data.RotY, data.RotZ);

            data.ColDCIndex = ColAndreasDynamicObjectManager.CreateObject_DC(data.Model, pos, rot, worldid: data.VirtualWorld, interiorid: data.Interior);

            var labelText = BuildLabelText(data.Id, data.Amount);
            data.Label = new DynamicTextLabel(labelText, Color.White, pos + new Vector3(0, 0, 1.2f), 4.0f);
            data.Label.World = data.VirtualWorld;
            data.Label.Interior = data.Interior;

            data.Polygon = PolygonManager.CreateCircularPolygon(data.PosX, data.PosY, data.PosZ, PolygonRadius);
            TrashGridManager.Add(id, data.PosX, data.PosY);
        }

        public static void SetAmount(int id, int amount)
        {
            if (!Trashes.TryGetValue(id, out var data)) return;
            data.Amount = Math.Clamp(amount, 0, MaxAmount);
            UpdateLabel(data);
            _ = SaveAmountAsync(id, data.Amount);
        }

        public static void CollectAmount(int id, int collected)
        {
            if (!Trashes.TryGetValue(id, out var data)) return;
            data.Amount = Math.Max(0, data.Amount - collected);
            UpdateLabel(data);
            _ = SaveAmountAsync(id, data.Amount);
        }

        private static void UpdateLabel(DynamicTrashData data)
        {
            if (data.Label == null) return;
            data.Label.Text = BuildLabelText(data.Id, data.Amount);
        }

        private static string BuildLabelText(int id, int amount)
            => $"{{C6E2FF}}[TRASH {id}]\n{{FFFFFF}}Jumlah: {{FFFF00}}{amount}/100\n{{FFFFFF}}Tekan '{{FF0000}}Y{{FFFFFF}}' untuk berinteraksi";

        public static int CheckPlayerInTrash(Player player)
        {
            var pos = player.Position;
            var inCell = TrashGridManager.GetInCell(pos.X, pos.Y);

            foreach (var id in inCell)
            {
                if (!Trashes.TryGetValue(id, out var data)) continue;
                if (data.Interior != player.Interior || data.VirtualWorld != player.VirtualWorld) continue;
                if (data.Polygon != null && data.Polygon.IsPointInside(pos))
                    return id;
            }

            return -1;
        }

        public static void HandleInteract(Player player)
        {
            if (!player.IsCharLoaded) return;
            var trashId = CheckPlayerInTrash(player);
            if (trashId == -1) return;
            // Reserved for Trashmaster job interaction
        }

        public static void StartEdit(Player player, int id)
        {
            if (!Trashes.TryGetValue(id, out var data)) return;

            var obj = ColAndreasDynamicObjectManager.GetDynamicObject(data.ColDCIndex);
            if (obj == null) return;

            _editingTrash[player.Id] = id;

            EventHandler<PlayerEditEventArgs> handler = null;
            handler = (sender, e) =>
            {
                if (e.Player?.Id != player.Id) return;
                if (e.Response == SampSharp.GameMode.Definitions.EditObjectResponse.Final)
                {
                    obj.Edited -= handler;
                    data.PosX = e.Position.X;
                    data.PosY = e.Position.Y;
                    data.PosZ = e.Position.Z;
                    data.RotX = e.Rotation.X;
                    data.RotY = e.Rotation.Y;
                    data.RotZ = e.Rotation.Z;
                    _editingTrash.Remove(player.Id);
                    _ = SaveAsync(id);
                    Rebuild(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Posisi Trash ID {id} berhasil diperbarui.");
                }
                else if (e.Response == SampSharp.GameMode.Definitions.EditObjectResponse.Cancel)
                {
                    obj.Edited -= handler;
                    obj.Position = new Vector3(data.PosX, data.PosY, data.PosZ);
                    obj.Rotation = new Vector3(data.RotX, data.RotY, data.RotZ);
                    _editingTrash.Remove(player.Id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Edit Trash dibatalkan.");
                }
            };

            obj.Edited += handler;
            obj.Edit(player);
        }

        public static DynamicTrashData GetTrash(int id) =>
            Trashes.TryGetValue(id, out var data) ? data : null;

        public static bool Exists(int id) => Trashes.ContainsKey(id);

        private static void DestroyObjects(DynamicTrashData data)
        {
            if (data.ColDCIndex >= 0) { ColAndreasDynamicObjectManager.DestroyObject(data.ColDCIndex); data.ColDCIndex = -1; }
            data.Label?.Dispose();
            data.Polygon?.Clear();
            TrashGridManager.Remove(data.Id, data.PosX, data.PosY);
        }

        private static int GetFreeId()
        {
            for (var i = 0; i < MaxTrashes; i++)
                if (!Trashes.ContainsKey(i)) return i;
            return -1;
        }
    }
}
