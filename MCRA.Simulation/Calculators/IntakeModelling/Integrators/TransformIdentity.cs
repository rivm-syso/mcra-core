using MCRA.Utils.Statistics;
using System;

namespace MCRA.Simulation.Calculators.IntakeModelling.Integrators {
    public class TransformIdentity : TransformBase {
        public override double Draw(IRandom random) {
            return NormalDistribution.DrawInvCdf(random, Mu, Math.Sqrt(VarianceBetween));
        }
    }
}
