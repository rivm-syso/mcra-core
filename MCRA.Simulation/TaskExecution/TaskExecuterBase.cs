using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;
using MCRA.General;
using MCRA.Utils.ProgressReporting;
using System.Reflection;

namespace MCRA.Simulation.TaskExecution
{

    public abstract class TaskExecuterBase : ITaskExecuter {

        protected readonly ILog _log = LogManager.GetLogger(typeof(Simulation));

        public TaskExecuterBase(string log4netConfigFile) {
            Log4netConfigFile = log4netConfigFile;
        }

        public string Log4netConfigFile { get; private set; }

        public abstract TaskExecutionResult Run(
            ITask task,
            CompositeProgressState progressReport
        );

        /// <summary>
        /// Initializes the log file for this run.
        /// </summary>
        protected void setupLogFile(int idTask) {
            ThreadContext.Properties["id"] = idTask;
            GlobalContext.Properties["id"] = idTask;

            string log4netConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Log4netConfigFile);
            XmlConfigurator.Configure(new FileInfo(log4netConfigPath));

            var rootAppender = ((Hierarchy)LogManager.GetRepository()).Root.Appenders.OfType<FileAppender>().FirstOrDefault();
            var logfile = rootAppender?.File ?? string.Empty;
            if (logfile != string.Empty && File.Exists(logfile)) {
                File.WriteAllText(logfile, string.Empty);
            }
        }


    }
}
