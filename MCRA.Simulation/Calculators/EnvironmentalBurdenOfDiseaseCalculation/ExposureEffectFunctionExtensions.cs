using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public static class ExposureEffectFunctionExtensions {

        public static double Compute(this ExposureEffectFunction eef, double x) {
            eef.Expression.Parameters["x"] = x;
            return (double)eef.Expression.Evaluate();
        }
    }
}
