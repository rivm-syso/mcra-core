using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class OIMDistributionSection : DistributionSectionBase {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

        public OIMDistributionSection() { }

        public OIMDistributionSection(bool isTotalDistribution, bool isAggregate) {
            IsTotalDistribution = isTotalDistribution;
            IsAggregate = isAggregate;
        }

        /// <summary>
        /// Upper distribution dietary.
        /// </summary>
        /// <param name="observedIndividualMeans"></param>
        /// <param name="percentageForUpperTail"></param>
        public void SummarizeUpperDietary(
            List<DietaryIndividualIntake> observedIndividualMeans,
            double percentageForUpperTail
        ) {
            var exposures = observedIndividualMeans.Select(c => c.DietaryIntakePerMassUnit).ToList();
            var weights = observedIndividualMeans.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualsId = observedIndividualMeans
                .Where(c => c.DietaryIntakePerMassUnit > intakeValue)
                .Select(c => c.SimulatedIndividualId)
                .ToHashSet();
            var upperIntakes = observedIndividualMeans
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .Select(c => c.DietaryIntakePerMassUnit)
                .ToList();
            var samplingWeights = observedIndividualMeans
                .Where(c => individualsId.Contains(c.SimulatedIndividualId))
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            UpperPercentage = 100 - percentageForUpperTail;
            CalculatedUpperPercentage = samplingWeights.Sum() / weights.Sum() * 100;
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
                .ToHashSet();
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
    }
}
