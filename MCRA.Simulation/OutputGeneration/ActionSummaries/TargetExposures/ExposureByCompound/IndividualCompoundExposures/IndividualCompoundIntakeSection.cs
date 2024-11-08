using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes an exposure matrix for chronic risk assessment (OIM) per substance including nondietary exposure if relevant.
    /// This table can be downloaded (limited to approx. 100,000 records).
    /// </summary>
    public sealed class IndividualCompoundIntakeSection : SummarySection {

        public int TruncatedIndividualsCount { get; set; }

        public List<IndividualCompoundIntakeRecord> IndividualCompoundIntakeRecords = [];

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit,
            Compound referenceSubstance = null,
            bool total = false
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var limit = 100000;
            var isCumulative = relativePotencyFactors != null;
            relativePotencyFactors = relativePotencyFactors
                ?? substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities
                ?? substances.ToDictionary(r => r, r => 1D);
            var results = new List<IndividualCompoundIntakeRecord>(limit);
            var summarizedIndividualsCount = 0;
            foreach (var idi in aggregateIndividualExposures) {
                if (total) {
                    var exposure = idi.GetTotalExternalExposureForSubstance(
                        referenceSubstance,
                        kineticConversionFactors,
                        externalExposureUnit.IsPerUnit()
                    );
                    if (exposure > 0) {
                        results.Add(new IndividualCompoundIntakeRecord() {
                            IndividualId = idi.Individual.Code,
                            NumberOfDaysInSurvey = idi.ExternalIndividualDayExposures.Count,
                            SamplingWeight = idi.IndividualSamplingWeight,
                            Bodyweight = idi.Individual.BodyWeight,
                            SubstanceCode = referenceSubstance?.Code,
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
                            results.Add(new IndividualCompoundIntakeRecord() {
                                IndividualId = idi.Individual.Code,
                                NumberOfDaysInSurvey = idi.ExternalIndividualDayExposures.Count,
                                SamplingWeight = idi.IndividualSamplingWeight,
                                Bodyweight = idi.Individual.BodyWeight,
                                SubstanceCode = substance?.Code,
                                Exposure = exposure,
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