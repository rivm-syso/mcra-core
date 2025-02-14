using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public class KineticConversionFactorsCalculator {

        private readonly ICollection<IKineticModelCalculator> _kineticConversionCalculator;

        public KineticConversionFactorsCalculator(
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators
        ) {
            _kineticConversionCalculator = kineticModelCalculators.Values;
        }

        public IDictionary<(ExposureRoute, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> exposureRoutes,
            ICollection<IExternalIndividualDayExposure> aggregateIndividualDayExposures,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            // TODO: How to compute absorption factors for metabolites?
            var result = new Dictionary<(ExposureRoute, Compound), double>();
            foreach (var instanceCalculator in _kineticConversionCalculator) {
                var fittedAbsorptionFactors = instanceCalculator
                    .ComputeAbsorptionFactors(
                        aggregateIndividualDayExposures,
                        exposureRoutes,
                        exposureUnit,
                        targetUnit,
                        generator
                    );
                foreach (var item in fittedAbsorptionFactors) {
                    result[(item.Key, instanceCalculator.Substance)] = item.Value;
                }
            }
            return result;
        }

        public IDictionary<(ExposureRoute, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> exposureRoutes,
            ICollection<IExternalIndividualExposure> aggregateIndividualExposures,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var result = new Dictionary<(ExposureRoute, Compound), double>();
            foreach (var instanceCalculator in _kineticConversionCalculator) {
                var inputSubstance = instanceCalculator.Substance;
                var fittedAbsorptionFactors = instanceCalculator
                    .ComputeAbsorptionFactors(
                        aggregateIndividualExposures,
                        exposureRoutes,
                        exposureUnit,
                        targetUnit,
                        generator
                    );
                foreach (var item in fittedAbsorptionFactors) {
                    result[(item.Key, inputSubstance)] = item.Value;
                }
            }
            return result;
        }
    }
}
