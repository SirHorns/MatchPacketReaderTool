namespace LeaguePacketsSerializer.Replication;

class Replicate
{
    public uint Uint;
    public float Float;
    public bool Bool;

    public Replicate(uint value)
    {
        Uint = value;
    }

    public Replicate(float value)
    {
        Float = value;
    }
    
    public Replicate(bool value)
    {
        Bool = value;
    }
}