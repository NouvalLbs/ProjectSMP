namespace ProjectSMP.Plugins.WeaponConfig
{
    public static partial class WeaponConfigService
    {
        public static void ForceRespawnFromDeath(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;

            s.DeathCts?.Cancel();
            s.IsDying = false;
            s.Health = s.MaxHealth;
            s.Armour = 0;

            p.VirtualWorld = s.IntendedWorld;
            p.ToggleControllable(true);

            PlayerDeathFinished?.Invoke(null, new DeathFinishedArgs { Player = p, Cancelable = true });
        }
    }
}