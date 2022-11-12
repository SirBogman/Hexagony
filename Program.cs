#nullable enable

using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagony
{
    static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            // Directly output characters up to and including 255 as bytes, without modifying them.
            // This allows UTF-8 encoded text to be output one byte at a time.
            Console.OutputEncoding = Encoding.GetEncoding("iso-8859-1");
            var bytes = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();
            var encodedBytes = Console.OutputEncoding
                .GetBytes(bytes.Select(x => (char)x).ToArray());
            if (!bytes.SequenceEqual(encodedBytes))
            {
                throw new InvalidOperationException("Encoding error.");
            }

            var generateOption = new Option<int>(
                new[] { "-g", "--generate" },
                "Generate a hexagon with the given size.");

            var debugOption = new Option<bool>(
                new[] { "-d", "--debug" },
                "Output debug information to STDERR for instructions preceded by \"`\".");

            var debugAllOption = new Option<bool>(
                new[] { "-D", "--debug-all" },
                "Output debug information to STDERR after every tick.");

            var fileArgument = new Argument<FileInfo>
            {
                Name = "File",
                Arity = ArgumentArity.ZeroOrOne,
                Description = "Path to code file. Use \"-\" for STDIN.",
            };

            var argumentsArgument = new Argument<string[]>
            {
                Name = "Arguments",
                Arity = ArgumentArity.ZeroOrMore,
                Description = "Optional arguments for program that will be joined with null characters. Otherwise, if STDIN is not used for the code file, it will be used for input.",
            };

            var rootCommand = new RootCommand
            {
                generateOption,
                debugOption,
                debugAllOption,
                fileArgument,
                argumentsArgument,
            };

            rootCommand.Description = "Hexagony interpreter";

            rootCommand.SetHandler(async (context) =>
                await MainTask(new CommandLineOptions
                {
                    Generate = context.ParseResult.GetValueForOption(generateOption),
                    Debug = context.ParseResult.GetValueForOption(debugOption),
                    DebugAll = context.ParseResult.GetValueForOption(debugAllOption),
                    File = context.ParseResult.GetValueForArgument(fileArgument),
                    Arguments = context.ParseResult.GetValueForArgument(argumentsArgument),
                }));

            return await rootCommand.InvokeAsync(args);
        }

        private class CommandLineOptions
        {
            public int Generate { get; set; }

            public string[] Arguments { get; set; } = Array.Empty<string>();
            
            public FileInfo? File { get; set; }

            public bool Debug { get; set; }
            
            public bool DebugAll { get; set; }
        }

        private static async Task<int> MainTask(CommandLineOptions options)
        {
            if (options.Generate != 0)
            {
                Console.WriteLine(new Grid(options.Generate));
                return 0;
            }

            if (options.File == null)
            {
                Console.Error.WriteLine("No file specified.");
                return 1;
            }

            string code;
            var stdinCode = options.File.Name == "-";

            if (stdinCode)
            {
                code = await Console.In.ReadToEndAsync();
            }
            else
            {
                using var stream = options.File.OpenText();
                code = await stream.ReadToEndAsync();
            }

            await using var inputStream = options.Arguments.Length > 0 ?
                new MemoryStream(Encoding.UTF8.GetBytes(string.Join('\0', options.Arguments) + "\0")) :
                !stdinCode ?
                    Console.OpenStandardInput() :
                    new MemoryStream(new byte[0]);

            var debugLevel = options.DebugAll ? 2 : options.Debug ? 1 : 0;
            var environment = new HexagonyEnv(code, inputStream, debugLevel);

            try
            {
                environment.Run();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }

            return 0;
        }
    }
}
