using System.Reflection;
using CommandLine;
using CommandLine.Text;
using ModuleDiagramCreator.DiagramCreators;

namespace ModuleDiagramCreator {
    internal class Program {
        public static int Main(string[] args) {
            var assembly = Assembly.GetExecutingAssembly();
            //set thread culture to invariant to fix the number format of double values etc.
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

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
            Console.WriteLine($"MCRA: Module diagram creator command line tool {version}");
            Console.WriteLine("Developed by Biometris, Wageningen University and research © 2025");

            int result;
            try {
                var parserResult = new Parser(c => {
                    c.HelpWriter = null;
                    c.CaseInsensitiveEnumValues = true;
                })
                    .ParseArguments<CreateOptions>(args);
                result = parserResult.MapResult(
                    (CreateOptions options) => Create(options),
                    errors => showCommandHelp(parserResult, errors)
                );
            } catch {
                result = 1;
            }

            return result;
        }

        private static int Create(CreateOptions options) {
            var creator = new GraphvizDiagramCreator();
            return creator.CreateToFile(options);
        }

        private static int showCommandHelp(ParserResult<CreateOptions> parserResult, IEnumerable<Error> errs) {

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
