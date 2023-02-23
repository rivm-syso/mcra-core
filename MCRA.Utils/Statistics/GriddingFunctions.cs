namespace MCRA.Utils {

    /// <summary>
    /// Utility functions
    /// </summary>
    public static class GriddingFunctions {

        /// <summary>
        /// Generates an enumeration of n values equally spread over the interval [min, max].
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IEnumerable<double> Arange(double min, double max, int n) {
            return Arange(min, max, (max - min) / (n - 1)).ToList();
        }

        /// <summary>
        /// Generates an enumeration over the interval [min, max] with the points spread with steps of step.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static IEnumerable<double> Arange(double min, double max, double step) {
            for (double i = min; i < max; i += step) {
                yield return i;
            }
            yield return max;
        }

        /// <summary>
        /// Generates an enumeration of n values over the interval [min, max] with the
        /// values equally spread in log-space.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IEnumerable<double> LogSpace(double min, double max, int n) {
            var logMin = Math.Log(min);
            var logMax = Math.Log(max);
            foreach (var logValue in Arange(logMin, logMax, n)) {
                yield return Math.Exp(logValue);
            }
        }

        /// <summary>
        /// Get a grid
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IEnumerable<double> LogSpacePercentage(double min, double max, int n) {
            min = min <= 0 ? 0.01 : min;
            var logMin = Math.Log(min);
            var logMax = Math.Log(max);
            var values = Arange(logMin, logMax, n).Select(v => Math.Exp(v) > max ? max : Math.Exp(v));
            return values;
        }

        /// <summary>
        /// Constructs a series of percentages between 0 and 100.
        /// Resolution of this series is higher at the tails.
        /// </summary>
        /// <returns></returns>
        public static double[] GetPlotPercentages() {
            var step = 0.5;
            var grid = new List<double>();
            var firstPart = LogSpacePercentage(0.001, 0.01, 5);
            var secondPart = LogSpacePercentage(0.01, 0.1, 5);
            grid.AddRange(firstPart);
            grid.AddRange(secondPart);
            grid.AddRange(Arange(step, 100 - step, step));
            grid.AddRange(secondPart.Select(p => 100 - p));
            grid.AddRange(firstPart.Select(p => 100 - p));
            return grid.Distinct().OrderBy(c => c).ToArray();
        }

        /// <summary>
        /// Returns round levels
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public static List<double> GetAutomaticLevels(double minimum, double maximum) {
            var logMin = Math.Floor(Math.Log10(minimum));
            var logMax = Math.Ceiling(Math.Log10(maximum));
            var nInterval = 5;
            var axisInterval = 0D;
            var range = logMax - logMin;
            if (range <= 1) {
                axisInterval = range / nInterval;
            } else if (range > 1 && range <= 5) {
                axisInterval = 1;
                nInterval = (int)(range / axisInterval);
            } else if (range > 5) {
                axisInterval = (range - range % nInterval) / nInterval;
            }
            var temp = new List<double>();
            for (int i = 0; i < nInterval + 1; i++) {
                temp.Add(Math.Pow(10, logMin + axisInterval * i));
            }
            return temp.Distinct().ToList();
        }
    }
}
