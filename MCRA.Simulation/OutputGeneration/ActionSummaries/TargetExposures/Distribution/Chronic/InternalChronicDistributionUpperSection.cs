using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total transformed exposure distribution in bins, is used for plotting the transformed exposure distribution
    /// </summary>
    public class InternalChronicDistributionUpperSection : InternalDistributionSectionBase, IIntakeDistributionSection {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }
        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit,
            double percentageForUpperTail
        ) {
            if (substances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors
                    ?? substances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities
                    ?? substances.ToDictionary(r => r, r => 1D);
            }

            var exposures = aggregateIndividualExposures
                            .Select(c => c
                                .GetTotalExposureAtTarget(
                                    targetUnit.Target,
                                    relativePotencyFactors,
                                    membershipProbabilities
                                )
                            )
                            .ToList();
            var weights = aggregateIndividualExposures
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualsId = aggregateIndividualExposures
                .Where(c => c
                    .GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    ) > intakeValue
                )
                .Select(c => c.SimulatedIndividualId)
                .ToHashSet();

            var upperIntakes = aggregateIndividualExposures
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .Select(c => (Exposure: c
                    .GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    ),
                    SamplingWeight: c.IndividualSamplingWeight
                ))
                .ToList();
            UpperPercentage = 100 - percentageForUpperTail;
            CalculatedUpperPercentage = upperIntakes.Select(c => c.SamplingWeight).Sum() / weights.Sum() * 100;
            LowPercentileValue = upperIntakes.Select(c => c.Exposure).DefaultIfEmpty(double.NaN).Min();
            HighPercentileValue = upperIntakes.Select(c => c.Exposure).DefaultIfEmpty(double.NaN).Max();
            NRecords = upperIntakes.Count;
            Summarize(upperIntakes);
        }
    }
}
