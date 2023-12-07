using System;
using Newtonsoft.Json.Linq;

namespace MatchPacketReaderTool.Models;

/// <summary>
/// 
/// </summary>
public class RawPacket
{
    private string _rawJson;
    private string _rawId;
    private string _rawPacket;
    private JObject _packetInfo;
    private string _time;
    private string _channelId;
    private string _rawChannel;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rawJson"></param>
    /// <param name="rawId"></param>
    /// <param name="packet"></param>
    /// <param name="time"></param>
    /// <param name="channelId"></param>
    /// <param name="rawChannel"></param>
    /// <param name="packetInfo"></param>
    public RawPacket(string rawJson, string rawId, string packet, string time, string channelId, string rawChannel, JObject packetInfo = null)
    {
        _rawJson = rawJson;
        _rawId = rawId;
        _rawPacket = packet;
        _time = time;
        _channelId = channelId;
        _rawChannel = rawChannel;
        
        if (packet != null)
        {
            _packetInfo = packetInfo;
        }
    }

    public string RawId
    {
        get => _rawId;
        set => _rawId = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Packet
    {
        get => _rawPacket;
        set => _rawPacket = value ?? throw new ArgumentNullException(nameof(value));
    }

    public JObject PacketInfo
    {
        get => _packetInfo;
        set => _packetInfo = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Time
    {
        get => _time;
        set => _time = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string ChannelId
    {
        get => _channelId;
        set => _channelId = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public string RawChannel
    {
        get => _rawChannel;
        set => _rawChannel = value ?? throw new ArgumentNullException(nameof(value));
    }
}