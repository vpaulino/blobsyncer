namespace CommandLine.ArgumentsParser
{
    /// <summary>
    /// A record that represents a parsed argument with a parameter name and its values.
    /// </summary>
    public record Argument(string Name, string[] Values);


    public class Arguments 
    {

        List<Argument> arguments = new List<Argument>();

        public Arguments(IEnumerable<Argument> arguments)
        {
            this.arguments = new     List<Argument>(arguments);
        }


        public Argument GetByName(string Name) 
        {
            return this.arguments.FirstOrDefault(a => a.Name == Name); 
        }
    }

    /// <summary>
    /// Delegate that defines the signature for a parameter handler.
    /// The handler receives the arguments array and a reference to the current index,
    /// and returns a parsed <see cref="Argument"/>.
    /// </summary>
    /// <param name="args">The full array of command-line arguments.</param>
    /// <param name="index">
    /// The current position in the arguments array.
    /// The handler should update this index to reflect the consumed arguments.
    /// </param>
    /// <returns>A parsed <see cref="Argument"/>.</returns>
    public delegate Argument ParameterHandler(string[] args, ref int index);

    /// <summary>
    /// An object-oriented command line parser that uses a list of parameter definitions.
    /// Each parameter is defined as a pair: the parameter name and a handler that parses it.
    /// </summary>
    public class CommandLineParser
    {
        private readonly List<(string ParameterName, ParameterHandler Handler)> _parameterHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class.
        /// </summary>
        /// <param name="parameterHandlers">
        /// A list of pairs where the key is the parameter name and the value is the handler.
        /// </param>
        public CommandLineParser()
        {
            _parameterHandlers = new List<(string ParameterName, ParameterHandler Handler)>();
        }

        public CommandLineParser AddParameter(string parameterName, ParameterHandler parseNames)
        {
            _parameterHandlers.Add((parameterName, parseNames));
            return this;
        }

        /// <summary>
        /// Parses the provided command-line arguments using the registered parameter handlers.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A list of parsed <see cref="Argument"/> objects.</returns>
        public Arguments Parse(string[] args)
        {
            var parsedArguments = new List<Argument>();
            
            // Loop through the entire arguments array.
            for (int i = 0; i < args.Length; i++)
            {
                // Check if the current token matches any registered parameter.
                foreach (var (paramName, handler) in _parameterHandlers)
                {
                    if (args[i].Equals(paramName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Call the handler which will parse the parameter and may advance the index.
                        Argument argument = handler(args, ref i);
                        parsedArguments.Add(argument);
                        break; // Move to the next argument in the outer loop.
                    }
                }
            }

            return new Arguments(parsedArguments);
        }
    }
}
