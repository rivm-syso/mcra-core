namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Draws a random value from the Logistic Normal distribution
    /// </summary>
    public sealed class LogisticNormalDistribution : Distribution {

        /// <summary>
        /// Parameter mu of the distribution.
        /// </summary>
        public double Mu { get; set; }

        /// <summary>
        /// Parameter sigma of the distribution.
        /// </summary>
        public double Sigma { get; set; }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <returns>A random draw from the distribution.</returns>
        public override double Draw(IRandom random) {
            return UtilityFunctions.ILogit(Sigma * NormalDistribution.InvCDF(0, 1, random.NextDouble()) + Mu);
        }
    }
}
