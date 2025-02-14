using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public class SingleValueRisksCalculator {

        public ICollection<SingleValueRiskCalculationResult> Compute(
            ICollection<ISingleValueDietaryExposure> exposures,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            TargetUnit exposureUnit,
            TargetUnit hazardCharacterisationsUnit
        ) {
            var result = new List<SingleValueRiskCalculationResult>();
            foreach (var exposureRecord in exposures) {
                var targetUnitCorrectionFactor = exposureUnit
                    .GetAlignmentFactor(
                        hazardCharacterisationsUnit,
                        exposureRecord.Substance?.MolecularMass ?? double.NaN,
                        double.NaN
                    );
                var exposure = targetUnitCorrectionFactor * exposureRecord.Exposure;
                hazardCharacterisations.TryGetValue(exposureRecord.Substance, out var hazardCharacterisation);
                var hazardCharacterisationValue = hazardCharacterisation?.Value ?? double.NaN;
                var exposureSource = new SingleValueDietaryExposureSource() {
                    Source = exposureRecord,
                    Route = ExposureRoute.Oral,
                };
                var record = new SingleValueRiskCalculationResult() {
                    Substance = exposureRecord.Substance,
                    Source = exposureSource,
                    Exposure = exposure,
                    HazardCharacterisation = hazardCharacterisationValue,
                    ExposureHazardRatio = exposure / hazardCharacterisationValue,
                    HazardExposureRatio = hazardCharacterisationValue / exposure,
                    Origin = hazardCharacterisation?.PotencyOrigin ?? PotencyOrigin.Unknown,
                };
                result.Add(record);
            }
            return result;
        }
    }
}
