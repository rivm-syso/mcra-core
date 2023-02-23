using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelAssistedDistributionSection : DistributionSectionBase {
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public double UpperPercentage { get; set; }
        public int NRecords { get; set; }

        public ModelAssistedDistributionSection() {
            IsTotalDistribution = true;
        }

        /// <summary>
        /// Upper distribution dietary.
        /// </summary>
        /// <param name="observedIndividualMeans"></param>
        /// <param name="upperPercentage"></param>
        public void SummarizeUpperDietary(
            List<DietaryIndividualIntake> observedIndividualMeans,
            double upperPercentage
        ) {
            var exposures = observedIndividualMeans.Select(c => c.DietaryIntakePerMassUnit).ToList();
            var weights = observedIndividualMeans.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, upperPercentage);
            var individualsId = observedIndividualMeans
                .Where(c => c.DietaryIntakePerMassUnit > intakeValue)
                .Select(c => c.SimulatedIndividualId)
                .ToList();
            var upperIntakes = observedIndividualMeans
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .Select(c => c.DietaryIntakePerMassUnit)
                .ToList();
            var samplingWeights = observedIndividualMeans
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            UpperPercentage = 100 - samplingWeights.Sum() / weights.Sum() * 100;
            LowPercentileValue = upperIntakes.DefaultIfEmpty(double.NaN).Min();
            HighPercentileValue = upperIntakes.DefaultIfEmpty(double.NaN).Max();
            NRecords = upperIntakes.Count;
            Summarize(upperIntakes, samplingWeights);
        }

        /// <summary>
        /// Upper distribution aggregate.
        /// </summary>
        /// <param name="aggregateIndividualExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUpperAggregate(
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) {
            var exposures = aggregateIndividualExposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = aggregateIndividualExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, upperPercentage);
            var individualsId = aggregateIndividualExposures
                .Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) > intakeValue)
                .Select(c => c.SimulatedIndividualId)
                .ToList();
            var upperIntakes = aggregateIndividualExposures
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .ToList();
            var samplingWeights = aggregateIndividualExposures
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            UpperPercentage = 100 - samplingWeights.Sum() / weights.Sum() * 100;
            LowPercentileValue = upperIntakes.DefaultIfEmpty(double.NaN).Min();
            HighPercentileValue = upperIntakes.DefaultIfEmpty(double.NaN).Max();
            NRecords = upperIntakes.Count;
            Summarize(upperIntakes, samplingWeights);
        }
    }
}
