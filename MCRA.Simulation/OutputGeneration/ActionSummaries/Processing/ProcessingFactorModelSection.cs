using MCRA.Simulation.Calculators.ProcessingFactorCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ProcessingFactorModelSection : SummarySection {

        public List<ProcessingFactorModelRecord> Records { get; set; }

        public void Summarize(ICollection<ProcessingFactorModel> processingFactors) {
            Records = processingFactors
                .Select(r => {
                    var distributionModel = r as IDistributionProcessingFactorModel;
                    return new ProcessingFactorModelRecord() {
                        FoodName = r.Food.Name,
                        FoodCode = r.Food.Code,
                        SubstanceName = r.Substance?.Name,
                        SubstanceCode = r.Substance?.Code,
                        ProcessingTypeCode = r.ProcessingType.Code,
                        ProcessingTypeName = r.ProcessingType.Name,
                        Nominal = r.GetNominalValue(),
                        BulkingBlending = r.ProcessingType.IsBulkingBlending ? "yes" : "no",
                        Distribution = distributionModel?.DistributionType.ToString(),
                        Mu = distributionModel?.Mu ?? double.NaN,
                        Sigma = distributionModel?.Sigma ?? double.NaN
                    };
                })
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ProcessingTypeName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ProcessingTypeCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
