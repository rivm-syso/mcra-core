using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Acute risk assessment
    /// Summarizes an exposure matrix for acute risk assessment, including nondietary exposure if relevant.
    /// This table can be downloaded (limited to approx. 100,000 records).
    /// </summary>
    public sealed class IndividualDaySubstanceExposureSection : SummarySection {

        public int TruncatedIndividualDaysCount { get; set; }

        public List<IndividualDaySubstanceExposureRecord> Records = [];

        public void Summarize(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit,
            Compound referenceSubstance = null,
            bool total = false
        ) {
            var limit = 100000;
            var results = new List<IndividualDaySubstanceExposureRecord>(limit);
            var isCumulative = relativePotencyFactors != null;
            relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);
            var summarizedIndividualDaysCount = 0;

            foreach (var idi in aggregateIndividualDayExposures) {
                var individual = idi.Individual;
                if (total) {
                    var exposure = idi.GetTotalExternalExposureForSubstance(
                        referenceSubstance,
                        kineticConversionFactors,
                        externalExposureUnit.IsPerUnit()
                    );
                    if (exposure > 0) {
                        results.Add(new IndividualDaySubstanceExposureRecord() {
                            SimulatedIndividualDayId = idi.SimulatedIndividualDayId.ToString(),
                            DietarySurveyIndividualCode = idi.Individual.Code,
                            DietarySurveyDayCode = idi.Day,
                            Bodyweight = individual.BodyWeight,
                            SamplingWeight = idi.IndividualSamplingWeight,
                            SubstanceCode = referenceSubstance.Code,
                            Exposure = exposure,
                            CumulativeExposure = isCumulative ? exposure : double.NaN
                        });
                    }
                } else {
                    foreach (var substance in substances) {
                        var exposure = idi.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            externalExposureUnit.IsPerUnit()
                        );
                        if (exposure > 0) {
                            var cumulativeExposure = isCumulative
                                ? relativePotencyFactors[substance] * membershipProbabilities[substance] * exposure
                                : double.NaN;
                            results.Add(new IndividualDaySubstanceExposureRecord() {
                                SimulatedIndividualDayId = idi.SimulatedIndividualDayId.ToString(),
                                DietarySurveyIndividualCode = idi.Individual.Code,
                                DietarySurveyDayCode = idi.Day,
                                Bodyweight = individual.BodyWeight,
                                SamplingWeight = idi.IndividualSamplingWeight,
                                SubstanceCode = substance.Code,
                                Exposure = exposure,
                                CumulativeExposure = cumulativeExposure
                            });
                        }
                    }
                }
                summarizedIndividualDaysCount++;
                if (results.Count > limit) {
                    TruncatedIndividualDaysCount = summarizedIndividualDaysCount;
                    break;
                }
            }
            Records = [.. results];
        }
    }
}
