using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling.Integrators {

    /// <summary>
    /// Zero distribution class. Fallback distribution of only zeros when probability is on the limit.
    /// </summary>
    public sealed class TransformFallback : TransformBase {
        public double Probability { get; set; }

        public override double Draw(IRandom random) {
            return Probability;
        }
        public override double CDF(double x) {
            throw new NotImplementedException();
        }
    }
}
