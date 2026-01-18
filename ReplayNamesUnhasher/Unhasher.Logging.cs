using ReplayNamesUnhasher.Replications;

namespace ReplayNamesUnhasher;

public partial class Unhasher
{
    private void DumpState(byte[] bytes, int i, ReplicationTypes replicationType, byte primaryId, byte secondaryId, Exception? e = null)
    {
        var s1 = $"[{replicationType}]: Index: {primaryId}; SID: {secondaryId};";
        var s2 = $"Bytes: byte[{bytes.Length}]; ReadPos: [{i}]; {{ {string.Join(", ", bytes)} }}; ";
        Console.WriteLine($"{s1}");
        Console.WriteLine($"{s2}");
        if (e is not null)
        {
            Console.WriteLine(e);
        }
    }    
}