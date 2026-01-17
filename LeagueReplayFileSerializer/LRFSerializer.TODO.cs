namespace LeaguePacketsSerializer;

public partial class LRFSerializer
{
    /*private void ParseChunkPacket(Chunk chunk, ENetPacket enetPacket)
{
    if (enetPacket.Channel >= 8)
    {
        return;
    }

    int rawId = enetPacket.Bytes[0];
    if (rawId == 254)
    {
        rawId = enetPacket.Bytes[5] | enetPacket.Bytes[6] << 8;
    }

    try
    {
        var basePacket = BasePacket.Create(enetPacket.Bytes, (ChannelID)enetPacket.Channel);
        object packetToSerialize = basePacket;

        if (basePacket is OnReplication replication)
        {
            packetToSerialize = OnReplication(replication);
        }
        else
        {
            SetReplicationType(basePacket);
        }

        var serializedPacket = CreateSerializePacket(rawId, packetToSerialize, enetPacket);
        chunk.SerializedPackets.Add(serializedPacket);
        if (enetPacket.Channel > 0 && basePacket.ExtraBytes.Length > 0)
        {
            var softBad = SoftBad(rawId, enetPacket, basePacket);
            chunk.SoftBadPackets.Add(softBad);
        }

        if (basePacket is not IGamePacketsList list)
        {
            return;
        }

        var softBads = SoftBadLoop(list, enetPacket);
        foreach (var softBad in softBads)
        {
            chunk.SoftBadPackets.Add(softBad);
        }
    }
    catch (Exception exception)
    {
        var hardBad = HardBad(rawId, enetPacket, exception);
        chunk.HardBadPackets.Add(hardBad);
    }
}*/


}