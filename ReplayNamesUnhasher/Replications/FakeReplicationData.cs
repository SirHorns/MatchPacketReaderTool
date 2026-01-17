using System.Collections.Generic;

namespace LeaguePacketsSerializer.Replication;

public class FakeReplicationData
{
    public uint UnitNetID;
    public Dictionary<string, object> Data;

    public FakeReplicationData(uint netID, Dictionary<string, object> data)
    {
        UnitNetID = netID;
        Data = data;
    }
}