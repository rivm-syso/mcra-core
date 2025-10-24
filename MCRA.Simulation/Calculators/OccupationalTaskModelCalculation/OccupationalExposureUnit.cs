using MCRA.General;
using MCRA.General.UnitDefinitions.Units;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.OccupationalTaskModelCalculation {

    public class OccupationalExposureUnit {

        public SubstanceAmountUnit SubstanceAmountUnit { get; set; }

        public JobTaskExposureUnitDenominator Denominator { get; set; }

        public TimeUnit TimeUnit { get; set; }

        public JobTaskExposureEstimateType EstimateType { get; set; }

        public OccupationalExposureUnit() { }

        public OccupationalExposureUnit(
            SubstanceAmountUnit substanceAmountUnit,
            JobTaskExposureUnitDenominator denominator,
            JobTaskExposureEstimateType timeBasis,
            TimeUnit timeUnit
        ) {
            SubstanceAmountUnit = substanceAmountUnit;
            Denominator = denominator;
            EstimateType = timeBasis;
            TimeUnit = timeUnit;
        }

        public OccupationalExposureUnit(JobTaskExposureUnit jobTaskExposureUnit, JobTaskExposureEstimateType estimateType) {
            SubstanceAmountUnit = jobTaskExposureUnit.GetSubstanceAmountUnit();
            Denominator = jobTaskExposureUnit.GetUnitDenominator();
            EstimateType = estimateType;
            TimeUnit = jobTaskExposureUnit.GetTimeUnit();
        }

        public string GetShortDisplayName() {
            var result = SubstanceAmountUnit.GetShortDisplayName();
            if (Denominator != JobTaskExposureUnitDenominator.None) {
                result += $"/{Denominator.GetShortDisplayName()}";
            }
            if (TimeUnit != TimeUnit.NotSpecified) {
                result += $"/{TimeUnit.GetShortDisplayName()}";
            }
            return result;
        }

        public string GetDisplayName(bool includeEstimateType = true) {
            var result = SubstanceAmountUnit.GetShortDisplayName();
            if (Denominator != JobTaskExposureUnitDenominator.None) {
                result += $"/{Denominator.GetShortDisplayName()}";
            }
            if (TimeUnit != TimeUnit.NotSpecified) {
                result += $"/{TimeUnit.GetShortDisplayName()}";
            }
            if (includeEstimateType && EstimateType != JobTaskExposureEstimateType.Undefined) {
                result += $" - {EstimateType.GetShortDisplayName()}";
            }
            return result;
        }
    }
}
