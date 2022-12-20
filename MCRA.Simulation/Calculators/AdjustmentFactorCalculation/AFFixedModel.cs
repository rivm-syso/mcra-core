using MCRA.Utils.Statistics;
using System;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public sealed class AFFixedModel : AdjustmentFactorModelBase, IAdjustmentFactorModel {

        public double A { get; set; }


        public AFFixedModel(double a) {
            A = a;
            if (A < 0) {
                throw new Exception($"Fixed model: parameter A = {A} < 0. Restriction: value C > 0.");
            }
        }

        public override double DrawFromDistribution(IRandom random) {
            return A;
        }

        public override double GetNominal() {
            return A;
        }
    }
}
