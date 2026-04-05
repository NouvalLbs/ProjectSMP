namespace ProjectSMP.Plugins.RakNet
{
    public enum PR_ValueType
    {
        PR_INT8,
        PR_INT16,
        PR_INT32,
        PR_UINT8,
        PR_UINT16,
        PR_UINT32,
        PR_FLOAT,
        PR_BOOL,
        PR_STRING,
        PR_CINT8,
        PR_CINT16,
        PR_CINT32,
        PR_CUINT8,
        PR_CUINT16,
        PR_CUINT32,
        PR_CFLOAT,
        PR_CBOOL,
        PR_CSTRING,
        PR_BITS,
        PR_FLOAT3,
        PR_FLOAT4,
        PR_VECTOR,
        PR_NORM_QUAT,
        PR_STRING8,
        PR_STRING32,
        PR_IGNORE_BITS
    }

    public enum PR_PacketPriority
    {
        PR_SYSTEM_PRIORITY,
        PR_HIGH_PRIORITY,
        PR_MEDIUM_PRIORITY,
        PR_LOW_PRIORITY
    }

    public enum PR_PacketReliability
    {
        PR_UNRELIABLE = 6,
        PR_UNRELIABLE_SEQUENCED,
        PR_RELIABLE,
        PR_RELIABLE_ORDERED,
        PR_RELIABLE_SEQUENCED
    }

    public enum PR_EventType
    {
        PR_INCOMING_PACKET,
        PR_INCOMING_RPC,
        PR_OUTGOING_PACKET,
        PR_OUTGOING_RPC,
        PR_INCOMING_CUSTOM_RPC
    }
}