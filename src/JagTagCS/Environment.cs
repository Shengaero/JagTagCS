namespace JagTagCS;

public class Environment : Dictionary<string, object>
{
    public TValue? Get<TValue>(string key) where TValue : class
    {
        try
        {
            var value = this[key];
            return value as TValue;
        }
        catch(InvalidCastException)
        {
            return null;
        }
    }

    public TValue GetOrDefault<TValue>(string key, TValue defaultValue) where TValue : class
    {
        try
        {
            var value = this.GetValueOrDefault(key, defaultValue);
            return (TValue) value;
        }
        catch(InvalidCastException)
        {
            return defaultValue;
        }
    }
}
