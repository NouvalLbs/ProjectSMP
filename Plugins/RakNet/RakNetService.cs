#nullable enable
using System;

namespace ProjectSMP.Plugins.RakNet
{
    public static class RakNetService
    {
        private static RakNetNatives N => RakNetNatives.Instance;

        public static event Action<int, int, int>? OnIncomingPacket;
        public static event Action<int, int, int>? OnIncomingRPC;
        public static event Action<int, int, int>? OnOutgoingPacket;
        public static event Action<int, int, int>? OnOutgoingRPC;

        private static int Call(string func, string format, params object[] args)
            => N.CallRemoteFunction(func, format, args);

        public static void HandleIncomingPacket(int playerId, int packetId, int bs)
            => OnIncomingPacket?.Invoke(playerId, packetId, bs);

        public static void HandleIncomingRPC(int playerId, int rpcId, int bs)
            => OnIncomingRPC?.Invoke(playerId, rpcId, bs);

        public static void HandleOutgoingPacket(int playerId, int packetId, int bs)
            => OnOutgoingPacket?.Invoke(playerId, packetId, bs);

        public static void HandleOutgoingRPC(int playerId, int rpcId, int bs)
            => OnOutgoingRPC?.Invoke(playerId, rpcId, bs);

        public static int BS_New()
            => Call("RNB_BS_New", "");

        public static int BS_NewCopy(int bs)
            => Call("RNB_BS_NewCopy", "d", bs);

        public static void BS_Delete(int bs)
            => Call("RNB_BS_Delete", "d", bs);

        public static void BS_Reset(int bs)
            => Call("RNB_BS_Reset", "d", bs);

        public static void BS_ResetReadPointer(int bs)
            => Call("RNB_BS_ResetReadPointer", "d", bs);

        public static void BS_ResetWritePointer(int bs)
            => Call("RNB_BS_ResetWritePointer", "d", bs);

        public static void BS_IgnoreBits(int bs, int numberOfBits)
            => Call("RNB_BS_IgnoreBits", "dd", bs, numberOfBits);

        public static void BS_SetWriteOffset(int bs, int offset)
            => Call("RNB_BS_SetWriteOffset", "dd", bs, offset);

        public static int BS_GetWriteOffset(int bs)
            => Call("RNB_BS_GetWriteOffset", "d", bs);

        public static void BS_SetReadOffset(int bs, int offset)
            => Call("RNB_BS_SetReadOffset", "dd", bs, offset);

        public static int BS_GetReadOffset(int bs)
            => Call("RNB_BS_GetReadOffset", "d", bs);

        public static int BS_GetNumberOfBitsUsed(int bs)
            => Call("RNB_BS_GetNumberOfBitsUsed", "d", bs);

        public static int BS_GetNumberOfBytesUsed(int bs)
            => Call("RNB_BS_GetNumberOfBytesUsed", "d", bs);

        public static int BS_GetNumberOfUnreadBits(int bs)
            => Call("RNB_BS_GetNumberOfUnreadBits", "d", bs);

        public static int BS_GetNumberOfBitsAllocated(int bs)
            => Call("RNB_BS_GetNumberOfBitsAllocated", "d", bs);

        public static void BS_WriteInt8(int bs, int value)
            => Call("RNB_BS_WriteInt8", "dd", bs, value);

        public static void BS_WriteInt16(int bs, int value)
            => Call("RNB_BS_WriteInt16", "dd", bs, value);

        public static void BS_WriteInt32(int bs, int value)
            => Call("RNB_BS_WriteInt32", "dd", bs, value);

        public static void BS_WriteUint8(int bs, int value)
            => Call("RNB_BS_WriteUint8", "dd", bs, value);

        public static void BS_WriteUint16(int bs, int value)
            => Call("RNB_BS_WriteUint16", "dd", bs, value);

        public static void BS_WriteUint32(int bs, int value)
            => Call("RNB_BS_WriteUint32", "dd", bs, value);

        public static void BS_WriteFloat(int bs, float value)
            => Call("RNB_BS_WriteFloat", "df", bs, value);

        public static void BS_WriteBool(int bs, bool value)
            => Call("RNB_BS_WriteBool", "dd", bs, value ? 1 : 0);

        public static void BS_WriteString(int bs, string value)
            => Call("RNB_BS_WriteString", "ds", bs, value);

        public static int BS_ReadInt8(int bs)
            => Call("RNB_BS_ReadInt8", "d", bs);

        public static int BS_ReadInt16(int bs)
            => Call("RNB_BS_ReadInt16", "d", bs);

        public static int BS_ReadInt32(int bs)
            => Call("RNB_BS_ReadInt32", "d", bs);

        public static int BS_ReadUint8(int bs)
            => Call("RNB_BS_ReadUint8", "d", bs);

        public static int BS_ReadUint16(int bs)
            => Call("RNB_BS_ReadUint16", "d", bs);

        public static int BS_ReadUint32(int bs)
            => Call("RNB_BS_ReadUint32", "d", bs);

        public static float BS_ReadFloat(int bs)
        {
            int result = Call("RNB_BS_ReadFloat", "d", bs);
            return BitConverter.Int32BitsToSingle(result);
        }

        public static bool BS_ReadBool(int bs)
            => Call("RNB_BS_ReadBool", "d", bs) != 0;

        public static string BS_ReadString(int bs, int maxSize = 256)
        {
            var output = new string(' ', maxSize);
            Call("RNB_BS_ReadString", "dsd", bs, output, maxSize);
            return output.TrimEnd('\0');
        }

        public static void PR_SendPacket(
            int bs,
            int playerId,
            PR_PacketPriority priority = PR_PacketPriority.PR_HIGH_PRIORITY,
            PR_PacketReliability reliability = PR_PacketReliability.PR_RELIABLE_ORDERED,
            int orderingChannel = 0)
            => Call("RNB_PR_SendPacket", "ddddd", bs, playerId, (int)priority, (int)reliability, orderingChannel);

        public static void PR_SendRPC(
            int bs,
            int playerId,
            int rpcId,
            PR_PacketPriority priority = PR_PacketPriority.PR_HIGH_PRIORITY,
            PR_PacketReliability reliability = PR_PacketReliability.PR_RELIABLE_ORDERED,
            int orderingChannel = 0)
            => Call("RNB_PR_SendRPC", "dddddd", bs, playerId, rpcId, (int)priority, (int)reliability, orderingChannel);

        public static void PR_EmulateIncomingPacket(int bs, int playerId)
            => Call("RNB_PR_EmulateIncomingPacket", "dd", bs, playerId);

        public static void PR_EmulateIncomingRPC(int bs, int playerId, int rpcId)
            => Call("RNB_PR_EmulateIncomingRPC", "ddd", bs, playerId, rpcId);
    }
}