using System.Collections.Generic;
using LeaguePackets;

namespace LeaguePacketsSerializer.Replication;

public class FakeOnReplication
{
    // BasePacket
    public byte[] ExtraBytes;

    // GamePacket
    public uint SenderNetID;

    // OnReplication
    public GamePacketID ID = GamePacketID.OnReplication;

    public uint SyncID;

    // replaced field
    public List<FakeReplicationData> ReplicationData = new List<FakeReplicationData>();
}