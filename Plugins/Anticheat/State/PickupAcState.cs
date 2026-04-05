namespace ProjectSMP.Plugins.Anticheat.State;

// Type: 0=none, 1=money, 2=health, 3=armour, 4=weapon(see Weapon field), 5=other
public class PickupAcState
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public int Type { get; set; }
    public int Weapon { get; set; }
    public int Amount { get; set; }
}