using System.Collections.ObjectModel;

namespace JagTagCS;

public class Parser
{
    private readonly Environment _environment = new();
    private readonly Dictionary<string, ParserMethod> _methods = new();

    private readonly long _iterations;
    private readonly int _maxLength;
    private readonly int _maxOutput;

    public Parser(Collection<ParserMethod> methods, long iterations, int maxLength, int maxOutput)
    {
        foreach(var method in methods)
        {
            _methods[method.Name] = method;
        }

        _iterations = iterations;
        _maxLength = maxLength;
        _maxOutput = maxOutput;
    }

    public object this[string key]
    {
        set
        {
            lock(this)
            {
                _environment[key] = value;
            }
        }
    }

    public void Clear()
    {
        lock(this)
        {
            _environment.Clear();
        }
    }

    public string Parse(string input)
    {
        lock(this)
        {
            var output = FilterEscapes(input);
            var count = 0;
            var lastOutput = "";
            while(!lastOutput.Equals(output) && count < _iterations && output.Length <= _maxLength)
            {
                lastOutput = output;
                var endIndex = output.IndexOf('}');
                var startIndex = endIndex == -1 ? -1 : output.LastIndexOf('{', endIndex);
                if(endIndex != -1 && startIndex != -1)
                {
                    var contents = output.Substring(startIndex + 1, endIndex);
                    string? result = null;
                    var splitIndex = contents.IndexOf(':');
                    if(splitIndex == -1)
                    {
                        var method = _methods.TryGetValue(contents.Trim(), out var value) ? value : null;
                        if(method != null)
                        {
                            try
                            {
                                result = method.ParseSimple(_environment);
                            }
                            catch(ParserException e)
                            {
                                return e.Message;
                            }
                        }
                    }
                    else
                    {
                        var name = contents[..splitIndex];
                        var parameters = contents[(splitIndex + 1)..];
                        var method = _methods.TryGetValue(name.Trim(), out var value) ? value : null;
                        if(method != null)
                        {
                            try
                            {
                                result = method.ParseComplex(_environment, UnfilterAll(parameters));
                            }
                            catch(ParserException e)
                            {
                                return e.Message;
                            }
                        }
                    }

                    result ??= '{' + contents + '}';
                    output = output[..startIndex] + FilterAll(result) + output[(endIndex + 1)..];
                }

                count++;
            }

            output = UnfilterAll(output);
            if(output.Length > _maxOutput)
            {
                output = output[.._maxOutput];
            }

            return output;
        }
    }

    private static string FilterEscapes(string input)
    {
        return input.Replace("\\{", "\u0012").Replace("\\|", "\u0013").Replace("\\}", "\u0014");
    }

    private static string UnfilterEscapes(string input)
    {
        return input.Replace("\u0012", "\\{").Replace("\u0013", "\\|").Replace("\u0014", "\\}");
    }

    private static string FilterAll(string input)
    {
        return FilterEscapes(input).Replace("{", "\u0015").Replace("}", "\u0016");
    }

    private static string UnfilterAll(string input)
    {
        return UnfilterEscapes(input).Replace("\u0015", "{").Replace("\u0016", "}");
    }
}
