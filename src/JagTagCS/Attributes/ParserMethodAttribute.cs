namespace JagTagCS.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ParserMethodAttribute : Attribute
{
    public readonly string Name;
    public readonly string[] Splitter;

    public ParserMethodAttribute(string name, params string[] splitter)
    {
        Name = name;
        Splitter = splitter;
    }
}
