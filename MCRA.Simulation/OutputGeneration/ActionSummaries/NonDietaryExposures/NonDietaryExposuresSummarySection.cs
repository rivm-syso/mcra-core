using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryExposuresSummarySection : ActionSummarySectionBase {

        public List<NonDietaryExposuresSummaryRecord> Records { get; set; }
        public List<NonDietaryExposuresPercentilesRecord> PercentileRecords { get; set; }
        public List<NonDietarySurveyPropertyRecord> NonDietarySurveyPropertyRecords { get; set; }
        public List<NonDietaryExposureProbabilityRecord> NonDietarySurveyProbabilityRecords { get; set; }
        public ExternalExposureUnit ExternalExposureUnit { get; set; }

        public void Summarize(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposuresBySurveys,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage
        ) {
            var results = new List<(NonDietarySurvey Survey, ExposureRoute Route, Compound Substance, List<double> Exposures)>();
            var exposures = nonDietaryExposuresBySurveys
                .SelectMany(surveyExposures => surveyExposures.Value
                    .Where(r => string.IsNullOrEmpty(r.Code))
                    .SelectMany(c => c.NonDietaryExposures)
                    .GroupBy(r => r.Compound)
                    .Where(g => substances.Contains(g.Key))
                    .Select(g => (
                        NonDietarySurvey: surveyExposures.Key,
                        Substance: g.Key,
                        Oral: g.Select(r => r.Oral).ToList(),
                        Dermal: g.Select(r => r.Dermal).ToList(),
                        Inhalation: g.Select(r => r.Inhalation).ToList()
                    )
                ))
                .ToList();
            var oralExposures = exposures.Select(c => (
                    NonDietarySurvey: c.NonDietarySurvey,
                    ExposureRoute: ExposureRoute.Oral,
                    Substance: c.Substance,
                    Oral: c.Oral
                ))
                .ToList();
            var dermalExposures = exposures.Select(c => (
                    NonDietarySurvey: c.NonDietarySurvey,
                    ExposureRoute: ExposureRoute.Dermal,
                    Substance: c.Substance,
                    Dermal: c.Dermal
                ))
                .ToList();
            var inhalationExposures = exposures.Select(c => (
                    NonDietarySurvey: c.NonDietarySurvey,
                    ExposureRoute: ExposureRoute.Inhalation,
                    Substance: c.Substance,
                    Inhalation: c.Inhalation
                ))
                .ToList();

            results.AddRange(oralExposures);
            results.AddRange(dermalExposures);
            results.AddRange(inhalationExposures);

            ExternalExposureUnit = results.First().Survey.ExposureUnit;

            Records = summarizeExposures(
                results,
                lowerPercentage,
                upperPercentage
            );

            PercentileRecords = summarizeBoxPlotRecords(results);

            NonDietarySurveyPropertyRecords = summarizeSurveyProperties(nonDietaryExposuresBySurveys);

            NonDietarySurveyProbabilityRecords = summarizeProbabilities(nonDietaryExposuresBySurveys);
        }

        private List<NonDietarySurveyPropertyRecord> summarizeSurveyProperties(IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposuresBySurveys) {
            var nonDietarySurveyPropertyRecords = nonDietaryExposuresBySurveys.Keys
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
            return nonDietarySurveyPropertyRecords;
        }

        private List<NonDietaryExposureProbabilityRecord> summarizeProbabilities(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposuresBySurveys
        ) {
            var results = new List<NonDietaryExposureProbabilityRecord>();
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

                results.Add(new NonDietaryExposureProbabilityRecord() {
                    ExposureSets = sets.Count,
                    Probability = proportionZero,
                    Code = nondietarySurvey.Key.Code,
                    Description = nondietarySurvey.Key.Description,
                });
            }
            return results;
        }

        private static List<NonDietaryExposuresSummaryRecord> summarizeExposures(
            ICollection<(NonDietarySurvey Survey, ExposureRoute Route, Compound Substance, List<double> Exposures)> nonDietaryExposures,
            double lowerPercentage,
            double upperPercentage
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var records = new List<NonDietaryExposuresSummaryRecord>();
            foreach (var (Survey, Route, Substance, Exposures) in nonDietaryExposures) {
                var positives = Exposures.Where(r => r > 0).ToList();

                var percentilesSampleConcentrations = positives.Any()
                    ? positives.Percentiles(percentages)
                    : percentages.Select(r => double.NaN).ToArray();

                var record = new NonDietaryExposuresSummaryRecord() {
                    NonDietarySurvey = Survey.Name,
                    Unit = Survey.ExposureUnit.GetShortDisplayName(),
                    SubstanceCode = Substance.Code,
                    SubstanceName = Substance.Name,
                    ExposureRoute = Route.GetShortDisplayName(),
                    TotalIndividuals = Exposures.Count,
                    MeanPositives = positives.Any() ? positives.Average() : double.NaN,
                    LowerPercentilePositives = percentilesSampleConcentrations[0],
                    MedianPositives = percentilesSampleConcentrations[1],
                    UpperPercentilePositives = percentilesSampleConcentrations[2],
                    PositiveIndividuals = positives.Count(),
                };
                records.Add(record);
            }
            return records;
        }

        private static List<NonDietaryExposuresPercentilesRecord> summarizeBoxPlotRecords(
            ICollection<(NonDietarySurvey Survey, ExposureRoute Route, Compound Substance, List<double> Exposures)> nonDietaryExposures
        ) {
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var records = new List<NonDietaryExposuresPercentilesRecord>();

            foreach (var (Survey, Route, Substance, Exposures) in nonDietaryExposures) {
                var positives = Exposures.Where(r => r > 0).ToList();

                var percentiles = positives.Any()
                    ? positives.Percentiles(percentages).ToList()
                    : percentages.Select(r => double.NaN).ToList();
                var outliers = positives.Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                        || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                        .Select(c => c).ToList();

                var record = new NonDietaryExposuresPercentilesRecord() {
                    Unit = Survey.ExposureUnit.GetShortDisplayName(),
                    NonDietarySurvey = Survey.Name,
                    MinPositives = positives.Any() ? positives.Min() : 0,
                    MaxPositives = positives.Any() ? positives.Max() : 0,
                    SubstanceCode = Substance.Code,
                    SubstanceName = Substance.Name,
                    ExposureRoute = Route.GetShortDisplayName(),
                    Percentiles = percentiles,
                    NumberOfMeasurements = positives.Count(),
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / Exposures.Count,
                    Outliers = outliers,
                };
                if (record.NumberOfMeasurements > 0) {
                    records.Add(record);
                }
            }
            return records;
        }
    }
}
