using System;
using System.Collections.Generic;
using System.IO;

namespace LeaguePacketsSerializer.ENet
{
    public abstract class ENetProtocol
    {
        public static readonly Dictionary<ENetProtocolCommand, int> CommandFullSize = new()
        {
            [ENetProtocolCommand.NONE] = 0,
            [ENetProtocolCommand.ACKNOWLEDGE] = 8,
            [ENetProtocolCommand.CONNECT] = 40,
            [ENetProtocolCommand.VERIFY_CONNECT] = 36,
            [ENetProtocolCommand.DISCONNECT] = 8,
            [ENetProtocolCommand.PING] = 4,
            [ENetProtocolCommand.SEND_RELIABLE] = 6,
            [ENetProtocolCommand.SEND_UNRELIABLE] = 8,
            [ENetProtocolCommand.SEND_FRAGMENT] = 24,
            [ENetProtocolCommand.SEND_UNSEQUENCED] = 8,
            [ENetProtocolCommand.BANDWIDTH_LIMIT] = 12,
            [ENetProtocolCommand.THROTTLE_CONFIGURE] = 16,
        };

        public static readonly Dictionary<ENetProtocolCommand, Func<ENetProtocolHeader, ENetProtocolCommandHeader, BinaryReader, ENetProtocol>> CommandConstructors = new()
        {
            [ENetProtocolCommand.ACKNOWLEDGE] = (p, c, r) => new ENetProtocolAcknowledge(p, c, r),
            [ENetProtocolCommand.CONNECT] = (p, c, r) => new ENetProtocolConnect(p, c, r),
            [ENetProtocolCommand.VERIFY_CONNECT] = (p, c, r) => new ENetProtocolVerifyConnect(p, c, r),
            [ENetProtocolCommand.DISCONNECT] = (p, c, r) => new ENetProtocolDisconnect(p, c, r),
            [ENetProtocolCommand.PING] = (p, c, r) => new ENetProtocolPing(p, c, r),
            [ENetProtocolCommand.SEND_FRAGMENT] = (p, c, r) => new ENetProtocolSendFragment(p, c, r),
            [ENetProtocolCommand.SEND_RELIABLE] = (p, c, r) => new ENetProtocolSendReliable(p, c, r),
            [ENetProtocolCommand.SEND_UNRELIABLE] = (p, c, r) => new ENetProtocolSendUnreliable(p, c, r),
            [ENetProtocolCommand.SEND_UNSEQUENCED] = (p, c, r) => new ENetProtocolSendUnsequenced(p, c, r),
            [ENetProtocolCommand.BANDWIDTH_LIMIT] = (p, c, r) => new ENetProtocolBandwidthLimit(p, c, r),
            [ENetProtocolCommand.THROTTLE_CONFIGURE] = (p, c, r) => new ENetProtocolThrottleConfigure(p, c, r),
        };
    }
}