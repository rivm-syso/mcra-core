using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ChronicSingleValueEstimatesSection : SummarySection {

        public List<ChronicSingleValueEstimatesRecord> Records { get; set; }

        public void Summarize(ICollection<NediSingleValueDietaryExposureResult> results) {
            Records = new List<ChronicSingleValueEstimatesRecord>();
            var resultsBySubstance = results
                .GroupBy(r => r.Substance)
                .Where(r => r.Count() > 1);
            foreach (var record in resultsBySubstance) {
                var orderedHighExposureResults = record.OrderByDescending(c => c.HighExposure).ToList();
                var foodHighRac1 = orderedHighExposureResults.FirstOrDefault();
                var foodHighRac2 = orderedHighExposureResults.Skip(1).FirstOrDefault();
                var exposure = foodHighRac1.HighExposure + (double.IsNaN(foodHighRac2.HighExposure) ? 0D : foodHighRac2.HighExposure);
                exposure += orderedHighExposureResults
                    .Where(c => c.Food != foodHighRac1.Food && c.Food != foodHighRac2.Food)
                    .Where(c => !double.IsNaN(c.Exposure))
                    .Sum(c => c.Exposure);
                Records.Add(new ChronicSingleValueEstimatesRecord() {
                    SubstanceName = record.Key.Name,
                    SubstanceCode = record.Key.Code,
                    CalculationMethod = foodHighRac1.CalculationMethod.GetShortDisplayName(),
                    Exposure = exposure
                });
            }
        }


        public void Summarize(ICollection<ChronicSingleValueDietaryExposureResult> results) {
            Records = new List<ChronicSingleValueEstimatesRecord>();
            var substances = results.Select(c => c.Substance).Distinct(c => c).ToList();
            foreach (var substance in substances) {
                Records.Add(new ChronicSingleValueEstimatesRecord() {
                    SubstanceName = substance.Name,
                    SubstanceCode = substance.Code,
                    CalculationMethod = results.First().CalculationMethod.GetShortDisplayName(),
                    Exposure = results.Where(c => c.Substance == substance).Where(c => !double.IsNaN(c.Exposure)).Sum(c => c.Exposure)
                });
            }
        }

    }
}
