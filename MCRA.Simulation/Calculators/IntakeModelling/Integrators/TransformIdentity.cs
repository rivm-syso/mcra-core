using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling.Integrators {
    public class TransformIdentity : TransformBase {
        public override double Draw(IRandom random) {
            return NormalDistribution.DrawInvCdf(random, Mu, Math.Sqrt(VarianceBetween));
        }
    }
}
