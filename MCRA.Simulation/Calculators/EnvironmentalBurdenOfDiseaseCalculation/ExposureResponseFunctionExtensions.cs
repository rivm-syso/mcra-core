using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public static class ExposureResponseFunctionExtensions {

        public static double Compute(this ExposureResponseFunction erf, double x) {
            if (x < erf.Baseline) {
                return 1D;
            } else if (erf.ExposureResponseType == ExposureResponseType.Function) {
                erf.ExposureResponseSpecification.Parameters["x"] = x;
                return (double)erf.ExposureResponseSpecification.Evaluate();
            } else if (erf.ExposureResponseType == ExposureResponseType.PerDoubling) {
                var doubFac = (double)erf.ExposureResponseSpecification.Evaluate();
                return Math.Pow(doubFac, Math.Log2(x / erf.Baseline));
            } else if (erf.ExposureResponseType == ExposureResponseType.PerUnit) {
                var a = (double)erf.ExposureResponseSpecification.Evaluate();
                var b = 1 - a * erf.Baseline;
                return a * x + b;
            } else {
                throw new NotImplementedException();
            }
        }
    }
}

