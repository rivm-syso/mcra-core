using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public static class ExposureEffectFunctionExtensions {

        public static double Compute(this ExposureEffectFunction eef, double x) {
            if (x < eef.Baseline) {
                return 1D;
            } else if (eef.ExposureResponseType == ExposureResponseType.Function) {
                eef.ExposureResponseSpecification.Parameters["x"] = x;
                return (double)eef.ExposureResponseSpecification.Evaluate();
            } else if (eef.ExposureResponseType == ExposureResponseType.PerDoubling) {
                var doubFac = (double)eef.ExposureResponseSpecification.Evaluate();
                return Math.Pow(doubFac, Math.Log2(x / eef.Baseline));
            } else if (eef.ExposureResponseType == ExposureResponseType.PerUnit) {
                var a = (double)eef.ExposureResponseSpecification.Evaluate();
                var b = 1 - a * eef.Baseline;
                return a * x + b;
            } else {
                throw new NotImplementedException();
            }
        }
    }
}

