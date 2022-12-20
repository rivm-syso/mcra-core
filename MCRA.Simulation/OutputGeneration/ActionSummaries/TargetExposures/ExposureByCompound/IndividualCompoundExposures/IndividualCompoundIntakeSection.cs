using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes an exposure matrix for chronic risk assessment (OIM) per substance including nondietary exposure if relevant.
    /// This table can be downloaded (limited to approx. 100.000 records).
    /// </summary>
    public sealed class IndividualCompoundIntakeSection : SummarySection {

        public int TruncatedIndividualsCount { get; set; }

        public List<IndividualCompoundIntakeRecord> IndividualCompoundIntakeRecords = new List<IndividualCompoundIntakeRecord>();

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance = null,
            bool total = false
        ) {
            var limit = 100000;
            var isCumulative = relativePotencyFactors != null;
            relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);
            var results = new List<IndividualCompoundIntakeRecord>(limit);
            var summarizedIndividualsCount = 0;
            foreach (var idi in aggregateIndividualExposures) {
                if (total) {
                    var exposure = idi.TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities);
                    if (exposure > 0) {
                        results.Add(new IndividualCompoundIntakeRecord() {
                            IndividualId = idi.Individual.Code,
                            NumberOfDaysInSurvey = idi.ExternalIndividualDayExposures.Count,
                            SamplingWeight = idi.IndividualSamplingWeight,
                            Bodyweight = idi.Individual.BodyWeight,
                            RelativeCompartmentWeight = idi.RelativeCompartmentWeight,
                            SubstanceCode = referenceSubstance?.Code,
                            ExposureAmount = exposure,
                            CumulativeExposure = isCumulative ? exposure : double.NaN
                        });
                    }
                } else {
                    foreach (var compound in substances) {
                        var exposure = idi.TargetExposuresBySubstance[compound].SubstanceAmount;
                        if (exposure > 0) {
                            var cumulativeExposure = isCumulative 
                                ? relativePotencyFactors[compound] * membershipProbabilities[compound] * exposure
                                : double.NaN;
                            results.Add(new IndividualCompoundIntakeRecord() {
                                IndividualId = idi.Individual.Code,
                                NumberOfDaysInSurvey = idi.ExternalIndividualDayExposures.Count,
                                SamplingWeight = idi.IndividualSamplingWeight,
                                Bodyweight = idi.Individual.BodyWeight,
                                RelativeCompartmentWeight = idi.RelativeCompartmentWeight,
                                SubstanceCode = compound?.Code,
                                ExposureAmount = exposure,
                                CumulativeExposure = cumulativeExposure
                            });
                        }
                    }
                    summarizedIndividualsCount++;
                    if (results.Count > limit) {
                        TruncatedIndividualsCount = summarizedIndividualsCount;
                        break;
                    }
                }
            }
            IndividualCompoundIntakeRecords = results.ToList();
        }
    }
}