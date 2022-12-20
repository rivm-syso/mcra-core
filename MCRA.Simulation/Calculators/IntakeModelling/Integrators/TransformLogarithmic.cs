using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling.Integrators {
    public sealed class TransformLogarithmic : TransformIdentity {
        public override double Draw(IRandom random) {
            return UtilityFunctions.ExpBound(base.Draw(random) + VarianceWithin / 2);
        }
    }
}
