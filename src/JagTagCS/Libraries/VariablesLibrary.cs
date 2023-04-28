using JagTagCS.Attributes;
using JagTagCS.Internal;

namespace JagTagCS.Libraries;

[Library(ReservedLibraryNames.Variables)]
public static class VariablesLibrary
{
    [ParserMethod("get")]
    public static string Get(Environment env, string[] input)
    {
        var variables = env.GetOrDefault("variables", new Dictionary<string, string>());
        return variables.GetValueOrDefault<string, string>(input[0], "");
    }

    [ParserMethod("set", "|")]
    public static string Set(Environment env, string[] input)
    {
        var variables = env.Get<Dictionary<string, string>>("variables");
        if(variables == null)
        {
            variables = new Dictionary<string, string>();
            env["variables"] = variables;
        }

        variables[input[0]] = input[1];
        return "";
    }
}
