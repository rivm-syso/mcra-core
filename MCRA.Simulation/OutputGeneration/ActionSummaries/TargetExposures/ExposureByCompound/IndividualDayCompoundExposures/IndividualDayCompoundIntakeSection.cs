using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Acute risk assessment
    /// Summarizes an exposure matrix for acute risk assessment, including nondietary exposure if relevant. 
    /// This table can be downloaded (limited to approx. 100.000 records).
    /// </summary>
    public sealed class IndividualDayCompoundIntakeSection : SummarySection {

        public int TruncatedIndividualDaysCount { get; set; }

        public List<IndividualDayCompoundIntakeRecord> IndividualCompoundIntakeRecords = new List<IndividualDayCompoundIntakeRecord>();

        public void Summarize(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance = null,
            bool total = false
        ) {
            var limit = 100000;
            var results = new List<IndividualDayCompoundIntakeRecord>(limit);
            var isCumulative = relativePotencyFactors != null;
            relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);
            var summarizedIndividualDaysCount = 0;
            foreach (var aggregateIndividualDayExposure in aggregateIndividualDayExposures) {
                var individual = aggregateIndividualDayExposure.Individual;
                if (total) {
                    var exposure = aggregateIndividualDayExposure.TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities);
                    if (exposure > 0) {
                        results.Add(new IndividualDayCompoundIntakeRecord() {
                            SimulatedIndividualDayId = aggregateIndividualDayExposure.SimulatedIndividualDayId.ToString(),
                            DietarySurveyIndividualCode = aggregateIndividualDayExposure.Individual.Code,
                            DietarySurveyDayCode = aggregateIndividualDayExposure.Day,
                            Bodyweight = individual.BodyWeight,
                            RelativeCompartmentWeight = aggregateIndividualDayExposure.RelativeCompartmentWeight,
                            SamplingWeight = aggregateIndividualDayExposure.IndividualSamplingWeight,
                            SubstanceCode = referenceSubstance.Code,
                            ExposureAmount = exposure,
                            CumulativeExposure = isCumulative ? exposure : double.NaN
                        });
                    }
                } else {
                    foreach (var compound in substances) {
                        var exposure = aggregateIndividualDayExposure.TargetExposuresBySubstance[compound].SubstanceAmount;
                        if (exposure > 0) {
                            var cumulativeExposure = isCumulative
                                ? relativePotencyFactors[compound] * membershipProbabilities[compound] * exposure
                                : double.NaN;
                            results.Add(new IndividualDayCompoundIntakeRecord() {
                                SimulatedIndividualDayId = aggregateIndividualDayExposure.SimulatedIndividualDayId.ToString(),
                                DietarySurveyIndividualCode = aggregateIndividualDayExposure.Individual.Code,
                                DietarySurveyDayCode = aggregateIndividualDayExposure.Day,
                                Bodyweight = individual.BodyWeight,
                                RelativeCompartmentWeight = aggregateIndividualDayExposure.RelativeCompartmentWeight,
                                SamplingWeight = aggregateIndividualDayExposure.IndividualSamplingWeight,
                                SubstanceCode = compound.Code,
                                ExposureAmount = exposure,
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
            IndividualCompoundIntakeRecords = results.ToList();
        }
    }
}
