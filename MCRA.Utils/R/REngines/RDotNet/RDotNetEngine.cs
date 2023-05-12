using System.Data;
using System.Text;
using RDotNet;

namespace MCRA.Utils.R.REngines {
    public class RDotNetEngine : IRCommandExecuter, IDisposable {

        /// <summary>
        /// Is the R script already running (doing some analysis).
        /// You have to wait till it is free again, otherwise commands may interfere.
        /// </summary>
        private static int _isRunning = 0;

        /// <summary>
        /// The R.Net engine.
        /// </summary>
        private static REngine _rEngine = null;

        private readonly bool _printDebug = false;

        /// <summary>
        /// Path to the R Home folder, e.g. C:\Program Files\R\R-4.2.1
        /// </summary>
        public static string R_HomePath { get; set; } = null;
        /// <summary>
        /// Path to the R executable, e.g. C:\Program Files\R\R-4.2.1\bin\x64
        /// If no value is given it defaults to the [R_HomePath]\bin\x64 folder
        /// </summary>
        public static string R_ExePath { get; set; } = null;
        /// <summary>
        /// Path to the R packages library, e.g. C:\Program Files\R\R-4.2.1\library
        /// If no value is given it defaults to the [R_HomePath]\library folder
        /// </summary>
        public static string R_LibraryPath { get; set; } = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RDotNetEngine(bool printDebug = false) {
            _printDebug = printDebug;
            start();
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~RDotNetEngine() {
            Dispose(false);
        }

        #region IDisposable

        /// <summary>
        /// Dispose implementation for IDisposable.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing) {
            stop();
        }

        #endregion

        #region Start/Stop/IsRunning

        /// <summary>
        /// Is the R script already running (doing some analysis).
        /// You have to wait till it is free again, otherwise commands may interfere.
        /// </summary>
        public bool IsRunning {
            get { return _isRunning != 0; }
        }

        /// <summary>
        /// Tell that you want to process some R-commands.
        /// This will block other threads to use R (if used properply)
        /// </summary>
        private void start() {
            if (Interlocked.Exchange(ref _isRunning, 1) == 1) {
                throw new Exception("Another instance of the R.Net engine is already running");
            }
            if (_rEngine == null) {
                try {
                    string rExePath = null;
                    string rLibPath = null;
                    string rHomePath = null;
                    if (!string.IsNullOrEmpty(R_HomePath) && Directory.Exists(R_HomePath)) {
                        rHomePath = R_HomePath;
                        //set defaults for exe and lib paths if not set explicitly
                        rExePath = string.IsNullOrEmpty(R_ExePath) ? Path.Combine(R_HomePath, "bin", "x64") : R_ExePath;
                        if (!Directory.Exists(rExePath)) {
                            rExePath = null;
                        }
                        rLibPath = string.IsNullOrEmpty(R_LibraryPath) ? Path.Combine(R_HomePath, "library") : R_LibraryPath;
                        if (!Directory.Exists(rLibPath)) {
                            rLibPath = null;
                        }
                    }
                    //set environment variable for the path to the R packages explicitly
                    Environment.SetEnvironmentVariable("R_LIBS_USER", rLibPath, EnvironmentVariableTarget.Process);
                    //set the exe and home path variables explicitly (if defined)
                    REngine.SetEnvironmentVariables(rExePath, rHomePath);
                    //System.Diagnostics.Debug.WriteLine("PATH=" + System.Environment.GetEnvironmentVariable("PATH"));
                    _rEngine = REngine.GetInstance();

                    // Workaround - explicitly include R libs in PATH so R environment can find them.  Not sure why R can't find them when
                    // we set this via Environment.SetEnvironmentVariable
                    var rExeEnvPath = rExePath.Replace('\\', '/');
                    _rEngine.Evaluate($"Sys.setenv(PATH = paste(\"{rExeEnvPath}\", Sys.getenv(\"PATH\"), sep=\";\"))");
                    // Reload stats package
                    _rEngine.Evaluate("library(stats)");

                    //only print R output when debugger is attached
                    _rEngine.AutoPrint = System.Diagnostics.Debugger.IsAttached;

#if DEBUG
                    //print r session info
                    var rInfo = EvaluateString("capture.output(sessionInfo())")
                              + $"\n\nLibrary Path: {EvaluateString(".libPaths()")}\n";
                    Console.WriteLine(rInfo);
#endif
                } catch {
                    Interlocked.Exchange(ref _isRunning, 0);
                    throw new Exception("Cannot find R on this computer");
                }
                evaluateCommand("options(width=10000)");
            }
            evaluateCommand("rm(list = ls())");
            _rEngine.AutoPrint = _printDebug;
        }

        /// <summary>
        /// Tell that you're ready with using R-commands. Always use this!
        /// </summary>
        private void stop() {
            if (_rEngine != null) {
                try {
                    var cmd = "rm(list = ls())";
                    evaluateCommand(cmd);
                } catch { }
            }
            Interlocked.Exchange(ref _isRunning, 0);
        }

        #endregion

        #region Evaluation interface

        /// <summary>
        /// Evaluates the given R command in the R environment.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private SymbolicExpression evaluateCommand(string command) {
            try {
                LogCommand(command);
                return _rEngine?.Evaluate(command);
            } catch (ParseException ex) {
                var sb = new StringBuilder();
                sb.AppendLine("Error while executing R script.");
                sb.Append($"Exception: {ex.Message}");
                throw new Exception(sb.ToString());
            } catch (Exception) {
                var sb = new StringBuilder();
                sb.AppendLine("Error while executing R script.");
                sb.Append($"Exception: {GetErrorMessage()}");
                throw new Exception(sb.ToString());
            }
        }

        #endregion

        #region Assignments and evaluations

        /// <summary>
        /// Evaluates the given R command in the R environment.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public void EvaluateNoReturn(string command) {
            evaluateCommand(command);
        }

        /// <summary>
        /// Evaluates the R variable as a boolean.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public void Comment(string message) {
            EvaluateNoReturn($"#{message}");
        }

        /// <summary>
        /// Assigns an integer in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetSymbol(string name, bool value) {
            var cmd = value ? $"{name} <- T" : $"{name} <- F";
            EvaluateNoReturn(cmd);
        }

        /// <summary>
        /// Assigns an integer in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetSymbol(string name, int value) {
            var cmd = $"{name} <- {value}";
            EvaluateNoReturn(cmd);
        }

        /// <summary>
        /// Assigns a double variable in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetSymbol(string name, double value) {
            var cmd = $"{name} <- {value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            EvaluateNoReturn(cmd);
        }

        /// <summary>
        /// Assigns an integer vector variable in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        public void SetSymbol(string name, IEnumerable<int> values) {
            var cmd = $"{name} <- c({string.Join(", ", values.Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture)))})";
            EvaluateNoReturn(cmd);
        }

        /// <summary>
        /// Assigns a multi-dimensional array of integers in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        public void SetSymbol(string name, int[,] values) {
            var rows = values.GetLength(0);
            var columns = values.GetLength(1);
            var flatValues = string.Join(", ", values.Cast<int>().Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            var cmd = $"{name} <- matrix(c({flatValues}), nrow = {rows}, ncol = {columns}, TRUE)";
            EvaluateNoReturn(cmd);
        }

        /// <summary>
        /// Assigns a numeric vector of doubles in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        public void SetSymbol(string name, IEnumerable<double> values) {
            var cmd = $"{name} <- c({string.Join(", ", values.Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture)))})";
            EvaluateNoReturn(cmd);
        }

        /// <summary>
        /// Assigns a multi-dimensional array of doubles in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        public void SetSymbol(string name, double[,] values) {
            var rows = values.GetLength(0);
            var columns = values.GetLength(1);
            var flatValues = string.Join(", ", values.Cast<double>().Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            var cmd = $"{name} <- matrix(c({flatValues}), nrow = {rows}, ncol = {columns}, TRUE)";
            EvaluateNoReturn(cmd);
        }

        /// <summary>
        /// Assigns a string variable in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetSymbol(string name, string value) {
            var cmd = $"{name} <- {value}";
            EvaluateNoReturn(cmd);
        }

        /// <summary>
        /// Assigns a list of strings variable in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        public void SetSymbol(string name, List<string> values) {
            var cmd = $"{name} <- c({string.Join(", ", values.Select(v => "'" + v + "'"))})";
            EvaluateNoReturn(cmd);
        }

        /// <summary>
        /// Assigns a data table as a data frame in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="table"></param>
        public void SetSymbol(string name, DataTable table) {
            var rows = table.Rows.Cast<DataRow>().ToList();
            var columns = table.Columns.Cast<DataColumn>().ToList();
            for (int i = 0; i < table.Columns.Count; ++i) {
                var column = table.Columns[i];
                if (column.DataType == typeof(double)) {
                    var values = rows.Select(r => (double)r[i]).ToList();
                    SetSymbol(escapeVariableName(column.ColumnName), values);
                } else if (column.DataType == typeof(int)) {
                    var values = rows.Select(r => (int)r[i]).ToList();
                    SetSymbol(escapeVariableName(column.ColumnName), values);
                } else {
                    var values = rows.Select(r => r[i].ToString()).ToList();
                    SetSymbol(escapeVariableName(column.ColumnName), values);
                    EvaluateNoReturn($"{escapeVariableName(column.ColumnName)} <- factor({escapeVariableName(column.ColumnName)})");
                }
            }
            EvaluateNoReturn($"{name} <- data.frame({ string.Join(", ", columns.Select(c => escapeVariableName(c.ColumnName))) })");
            var columnNames = columns.Select(r => r.ColumnName).ToList();
            SetSymbol($"colnames({ name })", columnNames);
        }

        private string escapeVariableName(string name) {
            var str = System.Text.RegularExpressions.Regex.Replace(name, @"\s+", ".");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"-", "");
            if (string.IsNullOrEmpty(str) || !char.IsLetter(str[0])) {
                return "c" + str;
            }
            return str;
        }

        /// <summary>
        /// Evaluates the R variable as a boolean.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool EvaluateBoolean(string name) {
            return evaluateCommand(name).AsLogical().First();
        }

        /// <summary>
        /// Evaluates the R variable as an integer.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int EvaluateInteger(string name) {
            return evaluateCommand(name).AsInteger().First();
        }

        /// <summary>
        /// Evaluates the R variable as a double.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double EvaluateDouble(string name) {
            return evaluateCommand(name).AsNumeric().First();
        }

        /// <summary>
        /// Evaluates the variable with the specified name as a string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string EvaluateString(string name) {
            var values = evaluateCommand(name).AsCharacter();
            return string.Join("\n", values);
        }

        /// <summary>
        /// Evaluates the R variable as a list of integers.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<int> EvaluateIntegerVector(string name) {
            var values = evaluateCommand(name).AsInteger();
            return values.ToList();
        }

        /// <summary>
        /// Evaluates the R variable as a two-dimensional array (i.e., a matrix)
        /// of integers.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int[,] EvaluateIntegerMatrix(string name) {
            var values = evaluateCommand(name).AsIntegerMatrix();
            return values.ToArray();
        }

        /// <summary>
        /// Evaluates the R variable as a list of doubles.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<double> EvaluateNumericVector(string name) {
            var values = evaluateCommand(name).AsNumeric();
            return values.ToList();
        }

        /// <summary>
        /// Evaluates the R variable as a two-dimensional array (i.e., a matrix)
        /// of integers.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double[,] EvaluateMatrix(string name) {
            var values = evaluateCommand(name).AsNumericMatrix();
            return values.ToArray();
        }

        /// <summary>
        /// Evaluates the R variable as a list of strings.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<string> EvaluateCharacterVector(string name) {
            var values = evaluateCommand(name).AsCharacter();
            return values.ToList();
        }

        /// <summary>
        /// Captures the output of the specified command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string CaptureOutput(string command) {
            var values = evaluateCommand(command: $"capture.output({command})").AsCharacter();
            return string.Join("\n", values);
        }

        /// <summary>
        /// Evaluates an R data frame as a datatable.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DataTable EvaluateDataTable(string name) {
            var dataset = evaluateCommand(name).AsDataFrame();
            var table = new DataTable();
            for (int i = 0; i < dataset.ColumnCount; ++i) {
                table.Columns.Add(dataset.ColumnNames[i], typeof(string));
            }
            for (int i = 0; i < dataset.RowCount; ++i) {
                var row = table.Rows.Add();
                for (int k = 0; k < dataset.ColumnCount; ++k) {
                    row[k] = dataset[i, k].ToString();
                }
            }
            return table;
        }

        /// <summary>
        /// Returns the latest error message of R.
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage() {
            return evaluateCommand("geterrmessage()").AsCharacter().First();
        }

        /// <summary>
        /// Returns the dll version of R that is used by this engine.
        /// </summary>
        /// <returns></returns>
        public string GetRVersion() {
            return _rEngine?.DllVersion ?? string.Empty;
        }

        /// <summary>
        /// Returns information about the used R version in a printable string.
        /// </summary>
        /// <returns></returns>
        public string GetRInfo() {
            var sb = new StringBuilder();
            var version = this.EvaluateString("R.version.string");
            sb.AppendLine(version);
            var rHome = this.EvaluateString("R.home(\"bin\")");
            sb.AppendLine($"R home: {rHome}");
            var libPaths = this.EvaluateCharacterVector(".libPaths()");
            sb.AppendLine($"Library paths: {string.Join(", ", libPaths)}");
            return sb.ToString();
        }

        public string InstalledPackages {
            get {
                EvaluateNoReturn("ip <- as.data.frame(installed.packages()[,c(1,3:4)])");
                EvaluateNoReturn("rownames(ip) <- NULL");
                EvaluateNoReturn("ip <- ip[is.na(ip$Priority),1:2,drop=FALSE]");
                var result = EvaluateString("capture.output(print(ip, row.names=FALSE))");
                return result;
            }
        }

        /// <summary>
        /// Tries to load the R package with the specified package name. If the package cannot be found
        /// locally, this method attempts to download and install it from the cran repository.
        /// </summary>
        /// <param name="packageName">The R package name.</param>
        /// <param name="minimalRequiredPackageVersion">The minimally required version.</param>
        /// <param name="autoFetchMissing">If true, this method should automatically download missing packages.</param>
        public void LoadLibrary(string packageName, Version minimalRequiredPackageVersion = null, bool autoFetchMissing = false) {
            Comment($"require('{packageName}')");
            var libLoaded = EvaluateBoolean(name: $"require('{packageName}')");
            if (!libLoaded) {
                if (autoFetchMissing) {
                    // TODO: set libPath/lib.loc according to static var R_LibraryPath
                    var libraryPath = this.EvaluateCharacterVector(".libPaths()").First();
                    try {
                        Comment($"Package {packageName} not found in R. Now trying to download and install it from cran.rVersions-project.org");
                        EvaluateNoReturn($"install.packages('{packageName}', repos='http://cran.r-project.org/', lib='{libraryPath}')");
                    } catch {
                        var message = $"R package {packageName} was not installed and could not be downloaded and installed from the cran website. Please install the package manually within R.";
                        Comment(message);
                        throw new Exception(message);
                    }
                    libLoaded = EvaluateBoolean($"require('{packageName}', lib.loc='{libraryPath}')");
                    if (libLoaded) {
                        Comment($"R package {packageName} is installed and loaded successfully.");
                    } else {
                        var message = $"Tried to download and install R package {packageName} but it could NOT be loaded. Please install the R package manually from within R.";
                        Comment(message);
                        throw new Exception(message);
                    }
                } else {
                    var msg = $"R package {packageName} could NOT be loaded. Please install the R package manually from within R.";
                    throw new Exception(msg);
                }
            } else {
                Comment($"R package {packageName} is loaded successfully.");
            }
            var libraryVersion = this.EvaluateString($"as.character(packageVersion('{packageName}'))");
            var installedPackageVersion = new Version(libraryVersion);
            if (minimalRequiredPackageVersion != null && installedPackageVersion < minimalRequiredPackageVersion) {
                var msg = $"Version of R package {packageName} (version {installedPackageVersion}) is too old. Please install later version (>= {minimalRequiredPackageVersion}).";
                Comment(msg);
                throw new Exception(msg);
            }
            Comment($"Package version of R package {packageName}: {installedPackageVersion}.");
        }

        #endregion

        /// <summary>
        /// Stub: this method should be implemented in derived classes to
        /// log R commands executed by this engine.
        /// </summary>
        /// <param name="command"></param>
        virtual protected void LogCommand(string command) {
            if (_printDebug && System.Diagnostics.Debugger.IsAttached) {
                System.Diagnostics.Debug.WriteLine(command);
            }
        }
    }
}
