namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Base class for statistical distribution like lognormal, normal, power
    /// </summary>
    public abstract class Distribution {

        public abstract double Draw(IRandom random);

        public abstract double CDF(double x);

        public virtual List<double> Draws(IRandom random, int n) {
            var draws = new List<double>();
            for (int i = 0; i < n; i++) {
                draws.Add(Draw(random));
            }
            return draws;
        }
    }
}
