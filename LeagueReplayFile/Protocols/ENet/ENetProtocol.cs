using LeagueReplayFile.Protocols.ENet.Protocols;

namespace LeagueReplayFile.Protocols.ENet;

public abstract class ENetProtocol
{
    protected virtual bool HandleProtocolHeader(ENetProtocolHeader header) => true;
    protected virtual bool HandleProtocolCommandHeader(ENetProtocolHeader protocol, ENetProtocolCommandHeader command) => true;
    protected virtual bool HandleProtocol(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader, ENetProtocolBase protocol) => true;

    

    protected void Read(BinaryReader reader, float timeReceived, ENetGameClientVersions version)
    {
        var protocolHeaderSize = reader.BytesLeft();
        if (protocolHeaderSize < ENetProtocolHeader.ProtocolHeaderSizes[version])
        {
            Console.WriteLine($"ProtocolHeader Size Mismatch; Got {protocolHeaderSize} , Expected {ENetProtocolHeader.ProtocolHeaderSizes[version]} for {version}");
            return;
        }
        
        var protocolHeader = ENetProtocolHeader.Read(reader, timeReceived, version);
        if (!HandleProtocolHeader(protocolHeader))
        {
            return;
        }
        
        while (reader.BytesLeft() > 0)
        {
            var commandHeaderSize = reader.BytesLeft();
            if (commandHeaderSize < ENetProtocolCommandHeader.CommandHeaderSize)
            {
                break;
            }
            var protocolCommandHeader = new ENetProtocolCommandHeader(reader);
            
            if (!ENetProtocolBase.CommandFullSize.TryGetValue(protocolCommandHeader.Command, out var fullSize))
            {
                break;
            }

            if (fullSize == 0 || reader.BytesLeft() < (fullSize - ENetProtocolCommandHeader.CommandHeaderSize))
            {
                break;
            }
            
            if(!HandleProtocolCommandHeader(protocolHeader, protocolCommandHeader))
            {
                break;
            }
            ENetProtocolBase protocol;
            try
            {
                protocol = ENetProtocolBase.CommandConstructors[protocolCommandHeader.Command](protocolHeader, protocolCommandHeader, reader);
            }
            catch (Exception)
            {
                //FIXME: optional strict flag
                break;
            }

            if (protocol == null)
            {
                continue;
            }
            
            if (!HandleProtocol(protocolHeader, protocolCommandHeader, protocol))
            {
                break;
            }
        }
    }
}