using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class TuningHackCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public TuningHackCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnVehicleMod(BaseVehicle vehicle, BasePlayer player, int componentId)
    {
        if (!_config.Enabled || !_config.GetCheck("TuningHack").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        if (!st.IsInModShop)
        {
            _warnings.AddWarning(player.Id, "TuningHack", $"comp={componentId} notInShop");
            return;
        }

        int price = TuningData.GetComponentPrice(componentId);
        if (price > 0 && player.Money < price)
            _warnings.AddWarning(player.Id, "TuningHack", $"comp={componentId} price={price} money={player.Money}");
    }
}