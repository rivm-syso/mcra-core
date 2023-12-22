using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryInputDataSection : ActionSummarySectionBase {

        public List<NonDietaryInputDataRecord> NonDietaryInputDataRecords { get; set; }
        public List<NonDietarySurveyPropertyRecord> NonDietarySurveyPropertyRecords { get; set; }
        public List<NonDietaryExposureProbabilityRecord> NonDietarySurveyProbabilityRecords { get; set; }

        public void Summarize(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposuresBySurveys,
            ICollection<Compound> substances) {
            NonDietaryInputDataRecords = nonDietaryExposuresBySurveys
                .SelectMany(surveyExposures => surveyExposures.Value
                    .Where(r => string.IsNullOrEmpty(r.Code))
                    .SelectMany(c => c.NonDietaryExposures)
                    .GroupBy(r => r.Compound)
                    .Where(g => substances.Contains(g.Key))
                    .Select(g => new NonDietaryInputDataRecord {
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

            NonDietarySurveyPropertyRecords = nonDietaryExposuresBySurveys.Keys
                .SelectMany(r => r.NonDietarySurveyProperties, (s, sp) => new NonDietarySurveyPropertyRecord() {
                    Code = s.Code,
                    Description = s.Description,
                    CovariateName = sp.IndividualProperty.Name,
                    PropertyType = sp.PropertyType.ToString(),
                    Minimum = sp.IndividualPropertyDoubleValueMin ?? double.NaN,
                    Maximum = sp.IndividualPropertyDoubleValueMax ?? double.NaN,
                    Level = sp.IndividualPropertyTextValue,
                })
                .ToList();

            NonDietarySurveyProbabilityRecords = new List<NonDietaryExposureProbabilityRecord>();
            foreach (var nondietarySurvey in nonDietaryExposuresBySurveys) {
                var sets = nondietarySurvey.Value
                    .Where(c => string.IsNullOrEmpty(c.Code))
                    .ToList();

                //this is not needed when data do not contain zeros, for backwards compatibility only
                var isExposure = 0d;
                foreach (var set in sets) {
                    var exposuresPerCompound = set.NonDietaryExposures.ToList();
                    isExposure += exposuresPerCompound.Sum(c => c.Oral + c.Inhalation + c.Dermal) > 0 ? 1 : 0;
                }
                var proportionZero = 100 - (sets.Count == 0 ? 0 : isExposure) / sets.Count * 100;
                if (!double.IsNaN(nondietarySurvey.Key.ProportionZeros)) {
                    proportionZero = nondietarySurvey.Key.ProportionZeros;
                } else {
                    nondietarySurvey.Key.ProportionZeros = 0;
                }

                NonDietarySurveyProbabilityRecords.Add(new NonDietaryExposureProbabilityRecord() {
                    ExposureSets = sets.Count,
                    Probability = proportionZero,
                    Code = nondietarySurvey.Key.Code,
                    Description = nondietarySurvey.Key.Description,
                });
            }
        }
    }
}
