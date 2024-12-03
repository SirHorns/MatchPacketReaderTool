using System;
using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public abstract class ENetProtocolHandler
{
    protected virtual bool HandleProtocolHeader(ENetProtocolHeader protocolHeader) => true;
    protected virtual bool HandleProtocolCommandHeader(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader) => true;
    protected virtual bool HandleProtocol(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, ENetProtocol protocol) => true;

    
    protected void Read(BinaryReader reader, float timeReceived, ENetLeagueVersion enetLeagueVersion)
    {
        if (reader.BytesLeft() < ENetProtocolHeader.ProtocolHeaderSizes[enetLeagueVersion])
        {
            return;
        }
        
        var protocolHeader = new ENetProtocolHeader(reader, timeReceived, enetLeagueVersion);
        if (!HandleProtocolHeader(protocolHeader))
        {
            return;
        }
        
        while (reader.BytesLeft() > 0)
        {
            if (reader.BytesLeft() < ENetProtocolCommandHeader.CommandHeaderSize)
            {
                break;
            }
            var protocolCommandHeader = new ENetProtocolCommandHeader(reader);
            
            if (!ENetProtocol.CommandFullSize.TryGetValue(protocolCommandHeader.Command, out var fullSize))
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
            ENetProtocol protocol;
            try
            {
                protocol = ENetProtocol.CommandConstructors[protocolCommandHeader.Command](protocolHeader, protocolCommandHeader, reader);
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