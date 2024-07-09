using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
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
            Compound substance,
            ICollection<KineticConversionFactorModel> kineticConversionFactorModel = null
        ) {
            var kineticConversionFactors = kineticConversionFactorModel?
                .Select(c => c.ConversionRule)
                .ToDictionary(c => (c.ExposurePathType, c.SubstanceFrom), c => c.ConversionFactor);

            var routes = absorptionFactors.Keys.Select(r => r.Route).Distinct();
            var result = new Dictionary<ExposurePathType, double>();
            foreach (var route in routes) {
                if ((kineticConversionFactors != null && kineticConversionFactors.TryGetValue((route, substance), out var factor))
                    || absorptionFactors.TryGetValue((route, substance), out factor)
                    || absorptionFactors.TryGetValue((route, null), out factor)
                    || absorptionFactors.TryGetValue((route, SimulationConstants.NullSubstance), out factor)
                ) {
                    result.Add(route, factor);
                }
            }
            return result;
        }

        /// <summary>
        /// Method to retrieve kinetic conversion factors from the conversion factors collection.
        /// Will first try to find a substance-specific absorption factor, but falls back
        /// to default absorption factor (of null-substance).
        /// </summary>
        /// <param name="kineticConversionFactorModels"></param>
        /// <param name="substance"></param>
        /// <returns></returns>
        public static ICollection<KineticConversionFactorModel> Get(
            this ICollection<KineticConversionFactorModel> kineticConversionFactorModels,
            Compound substance
        ) {
            var conversionFactorModels = kineticConversionFactorModels
                .Select(c => (
                    conversionRule: c.ConversionRule,
                    model: c
                )).ToDictionary(c => (c.conversionRule.ExposurePathType, c.conversionRule.TargetTo, c.conversionRule.SubstanceFrom), c => c.model);

            var routes = kineticConversionFactorModels
                .Select(c => c.ConversionRule.ExposurePathType)
                .Distinct()
                .ToList();
            var targetsTo = kineticConversionFactorModels
                .Select(c => c.ConversionRule.TargetTo)
                .Distinct()
                .ToList();
            var result = new List<KineticConversionFactorModel>();
            foreach (var route in routes) {
                foreach (var targetTo in targetsTo) {
                    if ((conversionFactorModels != null && conversionFactorModels.TryGetValue((route, targetTo, substance), out var model))
                    || conversionFactorModels.TryGetValue((route, targetTo, substance), out model)
                    || conversionFactorModels.TryGetValue((route, targetTo, null), out model)
                    || conversionFactorModels.TryGetValue((route, targetTo, SimulationConstants.NullSubstance), out model)
                ) {
                        result.Add(model);
                    }
                }
            }
            return result;
        }
    }
}
