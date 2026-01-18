namespace ReplayNamesUnhasher.Replications;

[AttributeUsage(AttributeTargets.Property)]
public class ReplicationAttribute : Attribute
{
    public int PID;
    public int SID;
}