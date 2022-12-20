using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NediSingleValueDietaryExposuresSection : SummarySection {

        public List<NediSingleValueDietaryExposuresRecord> Records { get; set; }

        public void Summarize(ICollection<NediSingleValueDietaryExposureResult> results) {
            Records = results
                .Select(r => {
                    return new NediSingleValueDietaryExposuresRecord() {
                        FoodName = r.Food.Name,
                        FoodCode = r.Food.Code,
                        ProcessingTypeCode = r.ProcessingType?.Code,
                        ProcessingTypeName = r.ProcessingType?.Name,
                        SubstanceName = r.Substance.Name,
                        SubstanceCode = r.Substance.Code,
                        CalculationMethod = r.CalculationMethod.GetShortDisplayName(),
                        MeanConsumption = r.MeanConsumption,
                        LargePortion = r.LargePortion,
                        ConcentrationValue = r.ConcentrationValue,
                        ConcentrationValueType = r.ConcentrationValueType.GetShortDisplayName(),
                        ProcessingFactor = r.ProcessingFactor,
                        Exposure = r.Exposure,
                        HighExposure = r.HighExposure,
                        OccurrenceFraction = r.OccurrenceFraction,
                        BodyWeight = r.BodyWeight
                    };
                })
                .OrderByDescending(c => c.Exposure)
                .ToList();
        }
    }
}
