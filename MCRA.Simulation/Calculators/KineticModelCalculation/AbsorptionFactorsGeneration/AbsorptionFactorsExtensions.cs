using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration {
    public static class AbsorptionFactorsExtensions {

        /// <summary>
        /// Method to retrieve absorption factors from the absorption factors dictionary.
        /// Will first try to find a substance-specific absorption factor, but falls back
        /// to default absorption factor (of null-substance).
        /// </summary>
        /// <param name="absorptionFactors"></param>
        /// <param name="substance"></param>
        /// <returns></returns>
        public static IDictionary<ExposurePathType, double> Get(
            this IDictionary<(ExposurePathType Route, Compound Substance), double> absorptionFactors,
            Compound substance
        ) {
            var routes = absorptionFactors.Keys.Select(r => r.Route).Distinct();
            var result = new Dictionary<ExposurePathType, double>();
            foreach (var route in routes) {
                if(absorptionFactors.TryGetValue((route, substance), out var factor) ||
                   absorptionFactors.TryGetValue((route, null), out factor) ||
                   absorptionFactors.TryGetValue((route, SimulationConstants.NullSubstance), out factor)
                ) {
                    result.Add(route, factor);
                }
            }
            return result;
        }
    }
}
