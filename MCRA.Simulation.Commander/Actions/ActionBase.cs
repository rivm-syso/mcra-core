using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Commander.Actions {
    public abstract class ActionBase {


        protected static void awaitDebugger(ActionOptionsBase options) {
#if DEBUG
            if (options.InteractiveMode) {
                Console.WriteLine("Please attach debugger, press enter to continue...");
                Console.ReadKey();
                Console.WriteLine();
            }
#endif
        }

        protected static ProgressReport createProgressReport(bool isSilent) {
            var progress = new ProgressReport();
            if (!isSilent) {
                var msgLength = 50;
                progress.ProgressStateChanged += (s, a) => {
                    var msg = $"  {((ProgressReport)s).CurrentActivity.PadRight(msgLength).Substring(0, msgLength)}| {((ProgressReport)s).Progress.ToString("N0")}%";
                    printConsole(msg, true);
                };
            }
            return progress;
        }

        protected static void printConsole(string msg, bool overwriteCurrentLine) {
            if (overwriteCurrentLine) {
                Console.SetCursorPosition(1, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(1, Console.CursorTop - 1);
            }
            Console.Write(msg);
        }
    }
}
