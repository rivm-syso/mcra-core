using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public class AFLognormalModel : AdjustmentFactorModelBase, IAdjustmentFactorModel {
        public double A { get; set; }

        public double B { get; set; }

        public double C { get; set; }

        public AFLognormalModel(double a, double b, double c) {
            A = a;
            B = b;
            C = c;
            if (A <= 0 || B <= 0 || C < 0) {
                throw new Exception($"LogNormal model: location parameter A = {A}, scale parameter B = {B}, offset parameter C = {C}. Restriction: A, B > 0; C >= 0.");
            }
        }

        public override double DrawFromDistribution(IRandom random) {
            var lognormal = new LogNormalDistribution(A, B, C);
            return lognormal.Draw(random);
        }

        public override double GetNominal() {
            var mean = Math.Exp(A) + C;
            return mean;
        }
    }
}
