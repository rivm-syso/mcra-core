using MCRA.Simulation.Calculators.KineticConversionFactorModels.KineticConversionFactorModelCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public class DerivedKineticConversionFactorModelsSummarySection
        : KineticConversionFactorModelsSummarySectionBase<DerivedKineticConversionFactorModelSummaryRecord> {

        public void Summarize(
            ICollection<KineticConversionFactorEmpiricalModel> conversionFactorModels
        ) {
            var records = new List<DerivedKineticConversionFactorModelSummaryRecord>();
            foreach (var model in conversionFactorModels) {
                var result = getSummaryRecord(model);
                result.ExposurePairs = model.KineticConversionDataRecords
                    .Select(r => new ExposurePair(r.FromExposure, r.ToExposure))
                    .ToList();
                records.Add(result);
            }
            Records = records;
        }
    }
}
