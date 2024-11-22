namespace LeaguePacketsSerializer.Replication;

class Replicate
{
    public uint Uint;
    public float Float;
    public bool IsFloat;

    public Replicate(uint value)
    {
        Uint = value;
        IsFloat = false;
    }

    public Replicate(float value)
    {
        Float = value;
        IsFloat = true;
    }
}