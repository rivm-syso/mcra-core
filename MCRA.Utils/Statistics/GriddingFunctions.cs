namespace MCRA.Utils {

    /// <summary>
    /// Utility functions
    /// </summary>
    public static class GriddingFunctions {

        /// <summary>
        /// Generates an enumeration of n values equally spread over the interval [min, max].
        /// NOTE: because of double rounding errors, the distance to the last value (max) can be very slightly off 
        /// from the distance (step size) between the other values.
        /// </summary>
        public static IEnumerable<double> Arange(double min, double max, int n) {
            var step = (max - min) / (n - 1);
            for (var i = 0; i < n - 1; i++) {
                yield return min + i * step;
            }
            yield return max;
        }

        /// <summary>
        /// Generates an enumeration of n values over the interval [min, max] with the
        /// values equally spread in log-space.
        /// </summary>
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
        public static double[] GetPlotPercentages() {
            var step = 0.5;
            var grid = new List<double>();
            var firstPart = LogSpacePercentage(0.001, 0.01, 5);
            var secondPart = LogSpacePercentage(0.01, 0.1, 5);
            grid.AddRange(firstPart);
            grid.AddRange(secondPart);
            grid.AddRange(Arange(step, 100 - step, 199));
            grid.AddRange(secondPart.Select(p => 100 - p));
            grid.AddRange(firstPart.Select(p => 100 - p));
            return grid.Distinct().Order().ToArray();
        }

        /// <summary>
        /// Returns round levels
        /// </summary>
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
