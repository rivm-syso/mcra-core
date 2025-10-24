using MCRA.General.UnitDefinitions.Units;

namespace MCRA.General {

    public static class JobTaskExposureUnitExtensions {

        public static SubstanceAmountUnit GetSubstanceAmountUnit(this JobTaskExposureUnit unit) {
            switch (unit) {
                case JobTaskExposureUnit.ugPerCm2PerDay:
                case JobTaskExposureUnit.ugPerM3:
                    return SubstanceAmountUnit.Micrograms;
                case JobTaskExposureUnit.mgPerCm2PerDay:
                case JobTaskExposureUnit.mgPerM3:
                    return SubstanceAmountUnit.Milligrams;
                default:
                    throw new Exception($"Failed to extract substance amount unit from job task exposure unit {unit}!");
            }
        }

        public static JobTaskExposureUnitDenominatorType GetUnitDenominatorType(this JobTaskExposureUnit unit) {
            switch (unit) {
                case JobTaskExposureUnit.mgPerCm2PerDay:
                case JobTaskExposureUnit.ugPerCm2PerDay:
                    return JobTaskExposureUnitDenominatorType.PerBodySurfaceArea;
                case JobTaskExposureUnit.ugPerM3:
                case JobTaskExposureUnit.mgPerM3:
                    return JobTaskExposureUnitDenominatorType.PerAirVolume;
                default:
                    throw new Exception($"Failed to extract unit denominator from job task exposure unit {unit}!");
            }
        }

        public static JobTaskExposureUnitDenominator GetUnitDenominator(this JobTaskExposureUnit unit) {
            switch (unit) {
                case JobTaskExposureUnit.ugPerCm2PerDay:
                case JobTaskExposureUnit.mgPerCm2PerDay:
                    return JobTaskExposureUnitDenominator.SquareCentimeters;
                case JobTaskExposureUnit.ugPerM3:
                case JobTaskExposureUnit.mgPerM3:
                    return JobTaskExposureUnitDenominator.CubicMeters;
                default:
                    throw new Exception($"Failed to extract unit denominator from job task exposure unit {unit}!");
            }
        }

        public static TimeUnit GetTimeUnit(this JobTaskExposureUnit unit) {
            switch (unit) {
                case JobTaskExposureUnit.ugPerCm2PerDay:
                case JobTaskExposureUnit.mgPerCm2PerDay:
                    return TimeUnit.Days;
                case JobTaskExposureUnit.ugPerM3:
                case JobTaskExposureUnit.mgPerM3:
                    return TimeUnit.NotSpecified;
                default:
                    throw new Exception($"Failed to extract time unit from job task exposure unit {unit}!");
            }
        }
    }
}
