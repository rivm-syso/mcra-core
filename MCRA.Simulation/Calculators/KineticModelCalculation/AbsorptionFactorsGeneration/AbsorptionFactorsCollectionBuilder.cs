using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration {
    public sealed class AbsorptionFactorsCollectionBuilder {
        private readonly IAbsorptionFactorsCollectionBuilderSettings _settings;

        public AbsorptionFactorsCollectionBuilder(IAbsorptionFactorsCollectionBuilderSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Creates a lookup dictionary of absorption factors for combinations
        /// of route and substance.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="substanceAbsorptionFactors"></param>
        /// <returns></returns>
        public IDictionary<(ExposurePathType, Compound), double> Create(
            ICollection<Compound> substances,
            ICollection<KineticAbsorptionFactor> substanceAbsorptionFactors = null
        ) {
            var kineticAbsorptionFactors = new Dictionary<(ExposurePathType, Compound), double> {
                [(ExposurePathType.Oral, SimulationConstants.NullSubstance)] = _settings.DefaultFactorDietary,
                [(ExposurePathType.Dermal, SimulationConstants.NullSubstance)] = _settings.DefaultFactorDermalNonDietary,
                [(ExposurePathType.Inhalation, SimulationConstants.NullSubstance)] = _settings.DefaultFactorInhalationNonDietary
            };
            if (substances != null) {
                if (substanceAbsorptionFactors != null) {
                    foreach (var item in substanceAbsorptionFactors) {
                        kineticAbsorptionFactors[(item.ExposureRoute, item.Compound)] = item.AbsorptionFactor;
                    }
                }
            }
            return kineticAbsorptionFactors;
        }
    }
}
