namespace ProjectSMP.Plugins.Anticheat.Configuration;

public enum PunishAction { Warn, Kick, Ban }

public class CheckConfig {
    public bool Enabled { get; set; } = true;
    public int MaxWarnings { get; set; } = 3;
    public PunishAction Action { get; set; } = PunishAction.Kick;
}