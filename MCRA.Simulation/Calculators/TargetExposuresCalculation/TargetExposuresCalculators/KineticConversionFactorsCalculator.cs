using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public class KineticConversionFactorsCalculator {

        private readonly IDictionary<Compound, IKineticConversionCalculator> _kineticModelCalculators;

        public KineticConversionFactorsCalculator(
            IDictionary<Compound, IKineticConversionCalculator> kineticModelCalculators
        ) {
            _kineticModelCalculators = kineticModelCalculators;
        }

        public IKineticConversionCalculator GetKineticConversionCalculator(
            Compound substance
        ) {
            if (_kineticModelCalculators.TryGetValue(substance, out var result)) {
                return result;
            } else {
                throw new Exception($"No kinetic conversion model found for substance {substance.Name} [{substance.Code}].");
            }
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
            foreach (var substance in substances) {
                var instanceCalculator = GetKineticConversionCalculator(substance);
                var fittedAbsorptionFactors = instanceCalculator
                    .ComputeAbsorptionFactors(
                        aggregateIndividualDayExposures,
                        exposureRoutes,
                        exposureUnit,
                        targetUnit,
                        generator
                    );
                foreach (var item in fittedAbsorptionFactors) {
                    result[(item.Key, substance)] = item.Value;
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
            foreach (var substance in substances) {
                var instanceCalculator = GetKineticConversionCalculator(substance);
                var fittedAbsorptionFactors = instanceCalculator
                    .ComputeAbsorptionFactors(
                        aggregateIndividualExposures,
                        exposureRoutes,
                        exposureUnit,
                        targetUnit,
                        generator
                    );
                foreach (var item in fittedAbsorptionFactors) {
                    result[(item.Key, substance)] = item.Value;
                }
            }
            return result;
        }
    }
}
