namespace ProjectSMP.Entities.Players.Administrator.Data
{
    public class Location
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public int Interior { get; set; }
        public int VirtualWorld { get; set; }
        public int City { get; set; }
    }

    public static class SendToData
    {
        public static readonly Location[] All = new[]
        {
            new Location { Name = "LS - Town Hall", X = 1481.0413f, Y = -1772.3140f, Z = 18.7958f, Interior = 0, VirtualWorld = 0, City = 1 },
            new Location { Name = "LS - Hospital", X = 1178.6931f, Y = -1323.3513f, Z = 14.0734f, Interior = 0, VirtualWorld = 0, City = 1 },
            new Location { Name = "LS - Police Department", X = 1545.9490f, Y = -1675.7349f, Z = 13.5554f, Interior = 0, VirtualWorld = 0, City = 1 },
            new Location { Name = "LS - Bank", X = 1457.2096f, Y = -1011.3639f, Z = 26.8438f, Interior = 0, VirtualWorld = 0, City = 1 },
            new Location { Name = "SF - City Hall", X = -2754.6145f, Y = 375.8407f, Z = 4.3347f, Interior = 0, VirtualWorld = 0, City = 2 },
            new Location { Name = "SF - Hospital", X = -2655.0654f, Y = 639.1252f, Z = 14.4531f, Interior = 0, VirtualWorld = 0, City = 2 },
            new Location { Name = "LV - Casino", X = 2193.9705f, Y = 1677.2203f, Z = 12.3672f, Interior = 0, VirtualWorld = 0, City = 3 },
            new Location { Name = "LV - Hospital", X = 1607.4803f, Y = 1817.7827f, Z = 10.8203f, Interior = 0, VirtualWorld = 0, City = 3 }
        };
    }
}