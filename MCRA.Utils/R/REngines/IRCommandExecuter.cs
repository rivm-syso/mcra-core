using System.Data;

namespace MCRA.Utils.R.REngines {
    public interface IRCommandExecuter {

        /// <summary>
        /// Evaluates the given R command in the R environment.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        void EvaluateNoReturn(string command);

        /// <summary>
        /// Assigns an integer in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetSymbol(string name, int value);

        /// <summary>
        /// Assigns a double variable in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetSymbol(string name, double value);

        /// <summary>
        /// Assigns an integer vector variable in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        void SetSymbol(string name, IEnumerable<int> values);

        /// <summary>
        /// Assigns a multi-dimensional array of integers in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        void SetSymbol(string name, int[,] values);

        /// <summary>
        /// Assigns a numeric vector of doubles in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        void SetSymbol(string name, IEnumerable<double> values);

        /// <summary>
        /// Assigns a multi-dimensional array of doubles in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        void SetSymbol(string name, double[,] values);

        /// <summary>
        /// Assigns a string variable in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        void SetSymbol(string name, List<string> values);

        /// <summary>
        /// Assigns a data table as a data frame in the R environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="table"></param>
        void SetSymbol(string name, DataTable table);

        /// <summary>
        /// Prints a comment line in R.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        void Comment(string message);

        /// <summary>
        /// Evaluates the R variable as a boolean.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool EvaluateBoolean(string name);

        /// <summary>
        /// Evaluates the R variable as an integer.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int EvaluateInteger(string name);

        /// <summary>
        /// Evaluates the R variable as a double.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        double EvaluateDouble(string name);

        /// <summary>
        /// Evaluates the variable with the specified name as a string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string EvaluateString(string name);

        /// <summary>
        /// Evaluates the R variable as a list of integers.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        List<int> EvaluateIntegerVector(string name);

        /// <summary>
        /// Evaluates the R variable as a two-dimensional array (i.e., a matrix)
        /// of integers.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int[,] EvaluateIntegerMatrix(string name);

        /// <summary>
        /// Evaluates the R variable as a list of doubles.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        List<double> EvaluateNumericVector(string name);

        /// <summary>
        /// Evaluates the R variable as a two-dimensional array (i.e., a matrix)
        /// of doubles.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        double[,] EvaluateMatrix(string name);

        /// <summary>
        /// Evaluates the R variable as a list of strings.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        List<string> EvaluateCharacterVector(string name);

        /// <summary>
        /// Captures the output of the specified command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        string CaptureOutput(string command);

        /// <summary>
        /// Evaluates an R data frame as a datatable.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        DataTable EvaluateDataTable(string name);

        /// <summary>
        /// Returns the latest error message of R.
        /// </summary>
        /// <returns></returns>
        string GetErrorMessage();

        /// <summary>
        /// Returns the dll version of R that is used by this engine.
        /// </summary>
        /// <returns></returns>
        string GetRVersion();

        /// <summary>
        /// Returns information about the used R version in a printable string.
        /// </summary>
        /// <returns></returns>
        string GetRInfo();

        /// <summary>
        /// Tries to load the R package with the specified package name. If the package cannot be found
        /// locally, this method attempts to download and install it from the cran repository.
        /// </summary>
        /// <param name="packageName">The R package name.</param>
        /// <param name="minimalRequiredPackageVersion">The minimally required package version.</param>
        /// <param name="autoFetchMissing">If true, this method should automatically download missing packages.</param>
        void LoadLibrary(string packageName, Version minimalRequiredPackageVersion = null, bool autoFetchMissing = false);

    }
}
