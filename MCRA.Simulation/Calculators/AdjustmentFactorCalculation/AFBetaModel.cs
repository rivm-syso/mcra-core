using MCRA.Utils.Statistics;
using System;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public class AFBetaModel : AdjustmentFactorModelBase, IAdjustmentFactorModel {

        public double A { get; set; }

        public double B { get; set; }

        public double C { get; set; }

        public double D { get; set; }

        public AFBetaModel(double a, double b, double c, double d) {
            A = a;
            B = b;
            C = c;
            D = d;
            if (C >= D) {
                throw new Exception($"Beta model: lowerbound parameter C = {C} is equal to upperbound D = {D}. Restriction: upperbound D > lowerbound C.");
            }
            if (C < 0 || D < 0) {
                throw new Exception($"Beta model: lowerbound parameter C = {C}, upperbound D = {D}. Restriction: C, D >= 0.");
            }
            if (A <= 0 || B <= 0) {
                throw new Exception($"Beta model: shape parameter A = {A}, shape parameter B = {B}. Restriction: A, B > 0.");
            }
        }
        public override double DrawFromDistribution(IRandom random) {
            var beta = new BetaDistribution(A, B);
            var draw = C + beta.Draw(random) * (D - C);
            return draw;
        }

        /// <summary>
        /// Returns an approximation of the nominal value of this
        /// model, which is the median, obtained from sampling
        /// 1000 values from this distribution.
        /// mean = C + A / (A + B) * (D - C)
        /// </summary>
        /// <returns></returns>
        public override double GetNominal() {
            var hash = (A + B + C + D).GetHashCode();
            var random = new McraRandomGenerator(hash);
            var beta = new ScaledBetaDistribution(A, B, C, D, random);
            var draws = beta.Draw(random, 1000);
            var median = draws.Median();
            return median;
        }
    }
}
