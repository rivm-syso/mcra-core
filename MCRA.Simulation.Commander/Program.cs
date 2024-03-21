using System.Reflection;
using CommandLine;
using CommandLine.Text;
using MCRA.Simulation.Commander.Actions.ConvertAction;
using MCRA.Simulation.Commander.Actions.CreateAction;
using MCRA.Simulation.Commander.Actions.RunAction;

namespace MCRA.Simulation.Commander {
    public class Program {
        public static int Main(string[] args) {
            var assembly = Assembly.GetExecutingAssembly();

            const string version = ThisAssembly.Git.BaseVersion.Major + "." +
                ThisAssembly.Git.BaseVersion.Minor + "." +
                ThisAssembly.Git.BaseVersion.Patch + "." +
                ThisAssembly.Git.Commits;

            var indent = new string(' ', 8);
            Console.WriteLine($@"{indent}       ______________");
            Console.WriteLine($@"{indent}      /\     ,,  ,,  \");
            Console.WriteLine($@"{indent}     /  \    ||\ ||\  \");
            Console.WriteLine($@"{indent}    / __ \   ||\\||\\  \");
            Console.WriteLine($@"{indent}   / //\\ \  || \\| \\  \");
            Console.WriteLine($@"{indent}  / //  || \_____________\");
            Console.WriteLine($@"{indent} / //      /   ______    /");
            Console.WriteLine($@"{indent} \ ||     /   //    ||  /  /\");
            Console.WriteLine($@"{indent}  \ \-,, /   //____//  /  //\\");
            Console.WriteLine($@"{indent}   \ `` /   //   ||   /  //  \\");
            Console.WriteLine($@"{indent}    \  /   //    ||  /  //====\\");
            Console.WriteLine($@"{indent}     \/_____________/  //      \\");
            Console.WriteLine();
            Console.WriteLine($"MCRA: Monte Carlo Risk Assessment Command line tool {version}");
            Console.WriteLine("Developed by Biometris, Wageningen University and research");

            int result;
            try {
                var parserResult = new Parser(c => {
                        c.HelpWriter = null;
                        c.CaseInsensitiveEnumValues = true;
                    })
                    .ParseArguments<RunActionOptions, ConvertActionOptions, CreateActionOptions>(args);
                result = parserResult.MapResult(
                    (RunActionOptions options) => runAction(options),
                    (ConvertActionOptions options) => runConvert(options),
                    (CreateActionOptions options) => runCreate(options),
                    errors => showCommandHelp(parserResult, errors)
                );
            } catch {
                result = 1;
            }

            return result;
        }

        private static int runAction(RunActionOptions options) {
            Console.WriteLine();
            var action = new RunAction();
            return action.Execute(options);
        }

        private static int runConvert(ConvertActionOptions options) {
            Console.WriteLine();
            var action = new ConvertAction();
            return action.Execute(options);
        }

        private static int runCreate(CreateActionOptions options) {
            Console.WriteLine();
            var action = new CreateAction();
            return action.Execute(options);
        }

        private static int showCommandHelp(ParserResult<object> parserResult, IEnumerable<Error> errs) {
            var text = HelpText.AutoBuild(parserResult, h => {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = string.Empty;
                return h;
            });
            Console.WriteLine(text);

            var result = -2;
            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError)) {
                result = -1;
            }
            return result;
        }
    }
}
