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
        public ICollection<SimpleAbsorptionFactor> Create(
            ICollection<Compound> substances,
            ICollection<SimpleAbsorptionFactor> substanceAbsorptionFactors = null
        ) {
            var kineticAbsorptionFactors = new List<SimpleAbsorptionFactor>() {
                new () { ExposureRoute = ExposurePathType.Oral, Substance = SimulationConstants.NullSubstance, AbsorptionFactor = _settings.DefaultFactorDietary },
                new () { ExposureRoute = ExposurePathType.Dermal, Substance = SimulationConstants.NullSubstance, AbsorptionFactor = _settings.DefaultFactorDermalNonDietary },
                new () { ExposureRoute = ExposurePathType.Inhalation, Substance = SimulationConstants.NullSubstance, AbsorptionFactor = _settings.DefaultFactorInhalationNonDietary }
            };

            if (substances != null) {
                if (substanceAbsorptionFactors != null) {
                    foreach (var item in substanceAbsorptionFactors) {
                        kineticAbsorptionFactors.Add(new () { ExposureRoute = item.ExposureRoute, Substance = item.Substance, AbsorptionFactor = item.AbsorptionFactor });
                    }
                }
            }
            return kineticAbsorptionFactors;
        }
    }
}
