using System.Reflection;
using LeaguePacketsSerializer.GameServer.Enums;
using LeaguePacketsSerializer.Replication;

namespace LeaguePacketsSerializer;


public partial class ReplicationDict
{
    private bool _recording;
    private uint _u { get; set; }
    private float _f { get; set; }
    private bool _b { get; set; }
    private ActionState _as { get; set; }
    private SpellDataFlags _sdf { get; set; }
    private ReplicateHold?[,] _currentValues { get; set; }
    private ReplicationType _currentReplicationType { get; set; }
    private ReplicationDataType?[][,] _replicationMaps { get; set; }
    
    
    
    /*private Dictionary<string, object> _test = new()
    {
        {"", ReplicationDataType.UNKNOWN},
        {"", ReplicationDataType.FLOAT},
        {"", ReplicationDataType.UINT},
        {"", ReplicationDataType.BOOL},
        {"", ReplicationDataType.ACTION_STATE},
        {"", ReplicationDataType.SPELL_DATA_FLAGS},
    };
    
    private Dictionary<string, object> Turret = new()
    {
        {"", ReplicationDataType.UNKNOWN},
        {"Stats.CurrentMana", ReplicationDataType.FLOAT},
        {"Stats.ActionState", ReplicationDataType.ACTION_STATE},
        {"", ReplicationDataType.BOOL},
        {"", ReplicationDataType.ACTION_STATE},
        {"", ReplicationDataType.SPELL_DATA_FLAGS},
    };*/
}