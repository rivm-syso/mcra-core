using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustExposuresSection : ActionSummarySectionBase {

        public List<DustExposuresDataRecord> DustExposuresDataRecords { get; set; }

        public void Summarize(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> dustExposuresBySurveys,
            ICollection<Compound> substances) {
            DustExposuresDataRecords = dustExposuresBySurveys
                .SelectMany(surveyExposures => surveyExposures.Value
                    .Where(r => string.IsNullOrEmpty(r.Code))
                    .SelectMany(c => c.NonDietaryExposures)
                    .GroupBy(r => r.Compound)
                    .Where(g => substances.Contains(g.Key))
                    .Select(g => new DustExposuresDataRecord {
                        NonDietarySurvey = surveyExposures.Key.Code,
                        CompoundName = g.Key.Name,
                        CompoundCode = g.Key.Code,
                        TotalIndividuals = g.Count(),
                        MeanDermal = g.Average(r => r.Dermal),
                        MeanOral = g.Average(r => r.Oral),
                        MeanInhalation = g.Average(r => r.Inhalation)
                    })
                    .ToList()
                )
                .ToList();
        }

        public void SummarizeUncertainty(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> dustExposures,
            double lowerBound,
            double upperBound
        ) {
            dustExposures
                .ForAll(surveyExposures => surveyExposures.Value
                    .SelectMany(r => r.NonDietaryExposures)
                    .GroupBy(r => r.Compound)
                    .ForAll(g => {
                        var record = DustExposuresDataRecords
                            .Where(r => r.CompoundCode == g.Key.Code)
                            .SingleOrDefault();
                        if (record != null) {
                            var meanDermal = g.Average(r => r.Dermal);
                            var meanInhalation = g.Average(r => r.Inhalation);
                            record.DustDermalUncertaintyValues.Add(meanDermal);
                            record.DustInhalationUncertaintyValues.Add(meanInhalation);
                        }
                    })
                );
        }
    }
}
