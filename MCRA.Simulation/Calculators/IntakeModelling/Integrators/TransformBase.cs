using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling.Integrators {
    public abstract class TransformBase : Distribution{
        public double VarianceBetween { get; set; }
        public double VarianceWithin { get; set; }
        public double Mu { get; set; }

        public override abstract double Draw(IRandom random);
    }
}
