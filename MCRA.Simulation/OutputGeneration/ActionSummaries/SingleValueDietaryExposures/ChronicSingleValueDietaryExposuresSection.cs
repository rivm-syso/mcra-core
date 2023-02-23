using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ChronicSingleValueDietaryExposuresSection : SummarySection {

        public List<ChronicSingleValueDietaryExposureRecord> Records { get; set; }

        public void Summarize(ICollection<ChronicSingleValueDietaryExposureResult> results) {
            Records = results
                .Select(r => {
                    return new ChronicSingleValueDietaryExposureRecord() {
                        FoodName = r.Food.Name,
                        FoodCode = r.Food.Code,
                        ProcessingTypeCode = r.ProcessingType?.Code,
                        ProcessingTypeName = r.ProcessingType?.Name,
                        SubstanceName = r.Substance.Name,
                        SubstanceCode = r.Substance.Code,
                        CalculationMethod = r.CalculationMethod.GetShortDisplayName(),
                        MeanConsumption = r.MeanConsumption,
                        ConcentrationValue = r.ConcentrationValue,
                        ConcentrationValueType = r.ConcentrationValueType.GetShortDisplayName(),
                        ProcessingFactor = r.ProcessingFactor,
                        Exposure = r.Exposure,
                        OccurrenceFraction = r.OccurrenceFraction,
                        BodyWeight = r.BodyWeight
                    };
                })
                .OrderByDescending(c => c.Exposure)
                .ToList();
        }
    }
}
