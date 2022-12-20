using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public class OIMDistributionSection : DistributionSectionBase {
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public double UpperPercentage { get; set; }
        public int NRecords { get; set; }
        public OIMDistributionSection() {
        }

        public OIMDistributionSection(bool isTotalDistribution, bool isAggregate) {
            IsTotalDistribution = isTotalDistribution;
            IsAggregate = isAggregate;
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
        /// Upper distribution NonDietary
        /// </summary>
        /// <param name="nonDietaryIndividualMeans"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUpperNonDietary(
            List<NonDietaryIndividualIntake> nonDietaryIndividualMeans,
            double upperPercentage,
            bool isPerPerson
        ) {
            var exposures = nonDietaryIndividualMeans.Select(c => c.NonDietaryIntakePerBodyWeight * (isPerPerson ? c.Individual.BodyWeight : 1)).ToList();
            var weights = nonDietaryIndividualMeans.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, upperPercentage);
            var individualsId = nonDietaryIndividualMeans
                .Where(c => c.NonDietaryIntakePerBodyWeight * (isPerPerson ? c.Individual.BodyWeight : 1) > intakeValue)
                .Select(c => c.SimulatedIndividualId)
                .ToList();
            var upperIntakes = nonDietaryIndividualMeans
                 .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                 .Select(c => c.NonDietaryIntakePerBodyWeight * (isPerPerson ? c.Individual.BodyWeight : 1))
                 .ToList();
            var samplingWeights = nonDietaryIndividualMeans
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            Summarize(upperIntakes, samplingWeights);
        }

        /// <summary>
        /// Upper distribution aggregate.
        /// </summary>
        /// <param name="aggregateIndividualMeans"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUpperAggregate(
            ICollection<AggregateIndividualExposure> aggregateIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) {
            var exposures = aggregateIndividualMeans.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = aggregateIndividualMeans.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, upperPercentage);
            var individualsId = aggregateIndividualMeans
                .Where(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)> intakeValue)
                .Select(c => c.SimulatedIndividualId)
                .ToList();
            var upperIntakes = aggregateIndividualMeans
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .ToList();
            var samplingWeights = aggregateIndividualMeans
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
