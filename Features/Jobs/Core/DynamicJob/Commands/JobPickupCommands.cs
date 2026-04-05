using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Commands;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Jobs.Core.DynamicJob;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Features.Jobs.Core.DynamicJob.Commands
{
    public class JobPickupCommands : AdminCommandBase
    {
        [Command("createjobp")]
        public static async void CreateJobPickup(Player player, string name, string jobName)
        {
            if (!CheckAdmin(player, 5)) return;

            var pos = player.Position;
            var id = await JobPickupService.CreateAsync(name, jobName, pos, player.VirtualWorld, player.Interior);

            if (id == -1)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Job pickup sudah mencapai batas maksimal!");
                return;
            }

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Job pickup '{name}' ({jobName}) berhasil dibuat dengan ID: {id}.");
        }

        [Command("gotojobp")]
        public static void GotoJobPickup(Player player, int id)
        {
            if (!CheckAdmin(player, 1) || !ValidateCharLoaded(player)) return;

            var job = JobPickupService.GetJob(id);
            if (job == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Job pickup ID {id} tidak ditemukan!");
                return;
            }

            player.SetPositionSafe(new Vector3(job.PosX, job.PosY, job.PosZ));
            player.SetInteriorSafe(job.Interior);
            player.SetVirtualWorldSafe(job.VirtualWorld);
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Teleport ke Job pickup ID {id}.");
        }

        [Command("editjobp")]
        public static async void EditJobPickup(Player player, int id, string type, string value = "")
        {
            if (!CheckAdmin(player, 5)) return;

            var job = JobPickupService.GetJob(id);
            if (job == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Job pickup ID {id} tidak ditemukan!");
                return;
            }

            switch (type.ToLower())
            {
                case "location":
                    var pos = player.Position;
                    job.PosX = pos.X;
                    job.PosY = pos.Y;
                    job.PosZ = pos.Z;
                    job.VirtualWorld = player.VirtualWorld;
                    job.Interior = player.Interior;
                    await JobPickupService.SaveAsync(id);
                    JobPickupService.Rebuild(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Location Job pickup ID {id} diperbarui.");
                    break;

                case "name":
                    job.Name = Utilities.ColouredText(value);
                    await JobPickupService.SaveAsync(id);
                    JobPickupService.Rebuild(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Nama Job pickup ID {id} diubah menjadi '{job.Name}'.");
                    break;

                case "jobname":
                    job.JobName = value;
                    await JobPickupService.SaveAsync(id);
                    JobPickupService.Rebuild(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Job name Job pickup ID {id} diubah menjadi '{job.JobName}'.");
                    break;

                case "delete":
                    await JobPickupService.DeleteAsync(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Job pickup ID {id} berhasil dihapus.");
                    break;

                default:
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd_G} Gunakan /editjobp [ID] [Prefix] [Value]");
                    player.SendClientMessage(Color.White, "{FF6347}>> Prefix{888888}: location, name, jobname, delete");
                    break;
            }
        }
    }
}