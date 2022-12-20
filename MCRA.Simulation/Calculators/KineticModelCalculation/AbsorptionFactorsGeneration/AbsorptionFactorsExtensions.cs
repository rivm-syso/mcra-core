using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Constants;
using System.Collections.Generic;
using System.Linq;

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
        public static Dictionary<ExposureRouteType, double> Get(
            this TwoKeyDictionary<ExposureRouteType, Compound, double> absorptionFactors,
            Compound substance
        ) {
            var routes = absorptionFactors.Keys.Select(r => r.Item1).Distinct();
            var result = new Dictionary<ExposureRouteType, double>();
            foreach (var route in routes) {
                var factors = absorptionFactors.Where(r => r.Key.Item1 == route && r.Key.Item2 == substance);
                if (!factors.Any()) {
                    factors = absorptionFactors.Where(r => r.Key.Item1 == route && r.Key.Item2 == null);
                }
                if (!factors.Any()) {
                    factors = absorptionFactors.Where(r => r.Key.Item1 == route && r.Key.Item2 == SimulationConstants.NullSubstance);
                }
                if (factors.Any()) {
                    result.Add(route, factors.First().Value);
                }
            }
            return result;
        }
    }
}
