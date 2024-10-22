using MCRA.Utils.Logger;

namespace MCRA.Utils.R.REngines {

    /// <summary>
    /// A dot net engine instance that includes a command logger for logging
    /// the R commands.
    /// </summary>
    public sealed class LoggingRDotNetEngine : RDotNetEngine {

        /// <summary>
        /// R commands logger.
        /// </summary>
        private readonly ILogger _commandLogger = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoggingRDotNetEngine(ILogger logger = null) : base() {
            _commandLogger = logger;
        }

        /// <summary>
        /// Evaluates the given R command in the R environment.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected override void LogCommand(string command) {
            _commandLogger?.Log(command);
        }
    }
}
