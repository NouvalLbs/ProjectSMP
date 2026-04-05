using System;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.RakNet
{
    public static class RakNetEventHandler
    {
        public static void Initialize()
        {
            RakNetService.OnIncomingPacket += HandleIncomingPacket;
            RakNetService.OnIncomingRPC += HandleIncomingRPC;
            RakNetService.OnOutgoingPacket += HandleOutgoingPacket;
            RakNetService.OnOutgoingRPC += HandleOutgoingRPC;
        }

        private static void HandleIncomingPacket(int playerId, int packetId, int bs)
        {
            var player = BasePlayer.Find(playerId) as Player;
            if (player == null) return;

            // Console.WriteLine($"[RakNet] Incoming Packet - Player: {player.Name}, PacketID: {packetId}, BS: {bs}");
        }

        private static void HandleIncomingRPC(int playerId, int rpcId, int bs)
        {
            var player = BasePlayer.Find(playerId) as Player;
            if (player == null) return;

            // Console.WriteLine($"[RakNet] Incoming RPC - Player: {player.Name}, RPCID: {rpcId}, BS: {bs}");
        }

        private static void HandleOutgoingPacket(int playerId, int packetId, int bs)
        {
            var player = BasePlayer.Find(playerId) as Player;
            if (player == null) return;

            // Console.WriteLine($"[RakNet] Outgoing Packet - Player: {player.Name}, PacketID: {packetId}, BS: {bs}");
        }

        private static void HandleOutgoingRPC(int playerId, int rpcId, int bs)
        {
            var player = BasePlayer.Find(playerId) as Player;
            if (player == null) return;

            // Console.WriteLine($"[RakNet] Outgoing RPC - Player: {player.Name}, RPCID: {rpcId}, BS: {bs}");
        }
    }
}