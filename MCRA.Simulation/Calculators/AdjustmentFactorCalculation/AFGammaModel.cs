using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public sealed class AFGammaModel : AdjustmentFactorModelBase, IAdjustmentFactorModel {
        public double A { get; set; }

        public double B { get; set; }

        public double C { get; set; }


        public AFGammaModel(double a, double b, double c) {
            A = a;
            B = b;
            C = c;
            if (A <= 0 || B <= 0 || C < 0) {
                throw new Exception($"Gamma model: shape parameter A = {A}, scale parameter B = {B}, offset parameter C = {C}. Restriction: A, B > 0; C >= 0.");
            }
        }

        public override double DrawFromDistribution(IRandom random) {
            var gamma = new GammaDistribution(A, B, C);
            return gamma.Draw(random);
        }

        /// <summary>
        /// Returns an approximation of the nominal value of this
        /// model, which is the median, obtained from sampling
        /// 1000 values from this distribution.
        /// Mean = A / B + C
        /// </summary>
        /// <returns></returns>
        public override double GetNominal() {
            var hash = (A + B + C).GetHashCode();
            var random = new McraRandomGenerator(hash);
            var gamma = new GammaDistribution(A, B, C);
            var draws = gamma.Draws(random, 1000);
            var median = draws.Median();
            return median;
        }
    }
}
