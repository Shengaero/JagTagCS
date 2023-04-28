namespace JagTagCS;

public abstract class ParserMethod
{
    public readonly string Name;

    protected ParserMethod(string name)
    {
        Name = name;
    }

    public abstract string? ParseSimple(Environment env);

    public abstract string? ParseComplex(Environment env, string input);

    public static ParserMethod Of(string name, Func<Environment, string> simple, params string[]? splitter)
    {
        return new InstanceParserMethod(name, simple, null, splitter);
    }

    public static ParserMethod Of(string name, Func<Environment, string[], string> complex, params string[]? splitter)
    {
        return new InstanceParserMethod(name, null, complex, splitter);
    }

    public static ParserMethod Of(string name, Func<Environment, string>? simple,
        Func<Environment, string[], string>? complex, params string[]? splitter)
    {
        return new InstanceParserMethod(name, simple, complex, splitter);
    }

    private class InstanceParserMethod : ParserMethod
    {
        private readonly Func<Environment, string>? _simple;
        private readonly Func<Environment, string[], string>? _complex;
        private readonly string[]? _splitter;

        public InstanceParserMethod(string name, Func<Environment, string>? simple,
            Func<Environment, string[], string>? complex, params string[]? splitter) : base(name)
        {
            _simple = simple;
            _complex = complex;
            _splitter = splitter;
        }

        public override string? ParseSimple(Environment env)
        {
            return _simple?.Invoke(env);
        }

        public override string? ParseComplex(Environment env, string input)
        {
            if(_complex == null)
            {
                return null;
            }

            string[] splitInput;
            if(_splitter == null)
            {
                splitInput = new[] { input };
            }
            else if(_splitter.Length == 0)
            {
                splitInput = input.Split("\\|");
            }
            else
            {
                splitInput = new string[_splitter.Length + 1];
                for(var i = 0; i < splitInput.Length - 1; i++)
                {
                    var index = input.IndexOf(_splitter[i], StringComparison.Ordinal);
                    if(index == -1)
                    {
                        return $"<invalid {Name} statement>";
                    }

                    splitInput[i] = input[..index];
                    input = input[(index + _splitter[i].Length)..];
                }

                splitInput[^1] = input;
            }

            return _complex.Invoke(env, splitInput);
        }
    }
}
