using System.Reflection;
using JagTagCS.Attributes;

namespace JagTagCS.Internal;

internal class ReflectionLookupUtil
{
    internal static ParserMethod[] GetMethods(Type type)
    {
        var libraryData = Attribute.GetCustomAttributes(type).FirstOrDefault(attribute => attribute is LibraryAttribute)
            is not LibraryAttribute libraryAttribute
            ? throw new ArgumentException($"Unable to load parser library of type: {type.Name}")
            : new LibraryData(type, libraryAttribute.Name);

        var parserMethodData = libraryData.Type.GetMethods()
            .Select(methodInfo =>
            {
                var attribute = methodInfo.GetCustomAttribute<ParserMethodAttribute>();
                if(attribute == null)
                {
                    return null;
                }

                if(!methodInfo.IsStatic)
                {
                    throw new ArgumentException(
                        $"Method {methodInfo.Name} had a ParserMethodAttribute but was not static. " +
                        "Only static methods are supported at this time!"
                    );
                }

                Func<Environment, string>? simple = null;
                Func<Environment, string[], string>? complex = null;
                var parameters = methodInfo.GetParameters();
                int paramCount;
                switch(paramCount = parameters.Length)
                {
                    case 0:
                        simple = _ =>
                            methodInfo.Invoke(null, null) as string
                            ?? throw new InvalidOperationException("Parser method function returned null!");
                        break;
                    case 1:
                    case 2:
                        if(parameters[0].ParameterType != typeof(Environment))
                        {
                            throw new ArgumentException(
                                $"Method {methodInfo.Name} had a ParserMethodAttribute but first" +
                                " parameter was not of type JagTagCS.Environment."
                            );
                        }

                        if(paramCount == 2)
                        {
                            if(parameters[1].ParameterType != typeof(string[]))
                            {
                                throw new ArgumentException(
                                    $"Method {methodInfo.Name} had a ParserMethodAttribute but second" +
                                    " parameter was not of type string[]."
                                );
                            }

                            complex = (environment, input) =>
                                methodInfo.Invoke(null, new object[] { environment, input }) as string
                                ?? throw new InvalidOperationException("Parser method function returned null!");
                        }
                        else
                        {
                            simple = environment =>
                                methodInfo.Invoke(null, new object[] { environment }) as string
                                ?? throw new InvalidOperationException("Parser method function returned null!");
                        }

                        break;
                    default:
                        throw new ArgumentException(
                            $"Method {methodInfo.Name} had a ParserMethodAttribute but requires ${paramCount} " +
                            "parameters. Only a maximum of 2 parameters are supported at this time!"
                        );
                }

                return new ParserMethodData(attribute.Name, simple, complex, attribute.Splitter);
            })
            .Select(data => data == null ? null : ParserMethod.Of(data.Name, data.Simple, data.Complex, data.Splitter))
            .Where(method => method != null)
            .ToArray() as ParserMethod[];
        return parserMethodData;
    }

    private class LibraryData
    {
        public readonly Type Type;
        public readonly string Name;

        public LibraryData(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    private class ParserMethodData
    {
        public readonly Func<Environment, string>? Simple;
        public readonly Func<Environment, string[], string>? Complex;
        public readonly string Name;
        public readonly string[]? Splitter;

        public ParserMethodData(string name, Func<Environment, string>? simple,
            Func<Environment, string[], string>? complex, string[]? splitter)
        {
            Simple = simple;
            Complex = complex;
            Name = name;
            Splitter = splitter;
        }
    }
}
