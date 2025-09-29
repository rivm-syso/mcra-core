using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.KineticConversionFactorCalculation {
    public class KineticConversionFactorsCalculator {

        private readonly KineticConversionCalculatorProvider _kineticConversionCalculatorCalculatorProvider;

        public KineticConversionFactorsCalculator(
            KineticConversionCalculatorProvider kineticConversionCalculatorCalculatorProvider
        ) {
            _kineticConversionCalculatorCalculatorProvider = kineticConversionCalculatorCalculatorProvider;
        }

        /// <summary>
        /// Computes acute kinetic conversion factors for the specified substances, exposure
        /// routes and exposure target based on the provided external individual exposures.
        /// </summary>
        public List<KineticConversionFactorResultRecord> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> exposureRoutes,
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var result = new List<KineticConversionFactorResultRecord>();
            foreach (var substance in substances) {
                var instanceCalculator = _kineticConversionCalculatorCalculatorProvider
                    .GetKineticConversionCalculator(substance);
                var derivedConversionFactors = instanceCalculator
                    .ComputeAbsorptionFactors(
                        externalIndividualDayExposures,
                        exposureRoutes,
                        exposureUnit,
                        targetUnit,
                        generator
                    );
                foreach (var item in derivedConversionFactors) {
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// Computes chronic kinetic conversion factors for the specified substances, exposure
        /// routes and exposure target based on the provided external individual exposures.
        /// </summary>
        public List<KineticConversionFactorResultRecord> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> exposureRoutes,
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var result = new List<KineticConversionFactorResultRecord>();
            foreach (var substance in substances) {
                var instanceCalculator = _kineticConversionCalculatorCalculatorProvider
                    .GetKineticConversionCalculator(substance);
                var derivedConversionFactors = instanceCalculator
                    .ComputeAbsorptionFactors(
                        externalIndividualExposures,
                        exposureRoutes,
                        exposureUnit,
                        targetUnit,
                        generator
                    );
                foreach (var item in derivedConversionFactors) {
                    result.Add(item);
                }
            }
            return result;
        }
    }
}
