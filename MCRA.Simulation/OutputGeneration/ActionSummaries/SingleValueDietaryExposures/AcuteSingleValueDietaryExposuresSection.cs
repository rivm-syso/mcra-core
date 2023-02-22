using MCRA.General;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AcuteSingleValueDietaryExposuresSection : SummarySection {

        public List<AcuteSingleValueDietaryExposureRecord> Records { get; set; }
        public SingleValueDietaryExposuresCalculationMethod SingleValueDietaryExposuresCalculationMethod { get; set; }

        public void Summarize(ICollection<AcuteSingleValueDietaryExposureResult> results, SingleValueDietaryExposuresCalculationMethod singleValueDietaryExposuresCalculationMethod) {
            SingleValueDietaryExposuresCalculationMethod = singleValueDietaryExposuresCalculationMethod;
            Records = results
                .Select(c => {
                    var unitWeightEpQualifiedValue = c.UnitWeightEp;
                    var unitWeightRacQualifiedValue = c.UnitWeightRac;
                    if (c.IESTICase == IESTIType.Case3) {
                        unitWeightEpQualifiedValue = null;
                        unitWeightRacQualifiedValue = null;
                    }
                    return new AcuteSingleValueDietaryExposureRecord() {
                        FoodName = c.Food.Name,
                        FoodCode = c.Food.Code,
                        ProcessingTypeName = c.ProcessingType?.Name,
                        ProcessingTypeCode = c.ProcessingType?.Code,
                        SubstanceName = c.Substance.Name,
                        SubstanceCode = c.Substance.Code,
                        LargePortion = c.LargePortion,
                        Exposure = c.Exposure,
                        CalculationMethod = c.IESTICase.GetDisplayName(),
                        ProcessingFactor = c.ProcessingFactor,
                        UnitWeightRacQualifiedValue = unitWeightRacQualifiedValue,
                        UnitWeightEpQualifiedValue = unitWeightEpQualifiedValue,
                        ConcentrationValue = c.ConcentrationValue,
                        ConcentrationValueType = c.ConcentrationValueType.GetShortDisplayName(),
                        UnitVariabilityFactor = c.UnitVariabilityFactor,
                        OccurrenceFraction = c.OccurrenceFraction,
                        BodyWeight = c.BodyWeight,

                    };
                })
                .OrderByDescending(c => c.Exposure)
                .ToList();
        }
    }
}
