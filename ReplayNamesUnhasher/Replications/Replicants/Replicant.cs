using System.Reflection;
using Newtonsoft.Json;

namespace ReplayNamesUnhasher.Replications;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class Replicant
{
    protected readonly PropertyInfo[] Properties;
    public Replicant()
    {
        var found  = GetType().GetProperties().Where(p => p.GetCustomAttribute(typeof(ReplicationAttribute)) != null);
        Properties = found.ToArray();
    }
    
    public void SetValue(int pid, int sid, object value)
    {
        foreach (var property in Properties)
        {
            var attribute = property.GetCustomAttribute(typeof(ReplicationAttribute));
            if (attribute is not ReplicationAttribute replicationAttribute)
            {
                continue;
            }

            if (replicationAttribute.PID != pid)
            {
                continue;
            }

            if (replicationAttribute.SID != sid)
            {
                continue;
            }

            var replicant = (Replicate)property.GetValue(this, null);
            if (replicant is null)
            {
                switch (this)
                {
                    case HQ hq:
                        break;
                    case Minion minion:
                        replicant = (Replicate)Activator.CreateInstance(property.PropertyType);
                        property.SetValue(minion, replicant, null);
                        break;
                }
            }
            if (replicant is null)
            {
                break;
            }
            replicant.Value = value;
            break;
        }
    }
}