using System.Threading;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public static partial class WeaponConfigService
    {
        public static void ResumeDeath(Player p, string animLib = "PED", string animName = "FLOOR_HIT")
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;

            s.IsDying = true;
            s.TrueDeath = false;
            s.Health = 0f;
            s.Armour = 0f;
            s.IntendedWorld = p.VirtualWorld;

            var prep = new PrepareDeathArgs
            {
                Player = p,
                AnimLib = animLib,
                AnimName = animName,
                AnimLock = true,
                RespawnTime = int.MaxValue
            };
            PlayerPrepareDeath?.Invoke(null, prep);
            if (prep.Cancel) return;

            s.DeathAnimLib = prep.AnimLib;
            s.DeathAnimName = prep.AnimName;

            p.ApplyAnimation(prep.AnimLib, prep.AnimName, 4.0f, false, false, false, prep.AnimLock, 0, true);
            p.ToggleControllable(false);
            p.ResetWeapons();

            var cts = new CancellationTokenSource();
            s.DeathCts = cts;
            _ = RunDeathAsync(p, s, prep.AnimLib, prep.AnimName, prep.AnimLock, prep.RespawnTime, cts.Token, cancelable: true);
        }
    }
}