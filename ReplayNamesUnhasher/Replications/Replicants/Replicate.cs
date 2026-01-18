namespace ReplayNamesUnhasher.Replications;

public class Replicate
{
    public object? Value { get; set; }
    public Replicate()
    {
        Value = null;
    }
}

public class Replicate<T>: Replicate
{
    public void SetValue(T value) => Value = value;
}