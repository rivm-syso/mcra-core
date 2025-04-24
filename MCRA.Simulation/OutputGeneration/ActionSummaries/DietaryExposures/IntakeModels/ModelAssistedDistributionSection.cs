using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Utils.Statistics;

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
        /// <param name="percentageForUpperTail"></param>
        public void SummarizeUpperDietary(
            List<DietaryIndividualIntake> observedIndividualMeans,
            double percentageForUpperTail
        ) {
            var exposures = observedIndividualMeans.Select(c => c.DietaryIntakePerMassUnit).ToList();
            var weights = observedIndividualMeans.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualsId = observedIndividualMeans
                .Where(c => c.DietaryIntakePerMassUnit > intakeValue)
                .Select(c => c.SimulatedIndividual.Id)
                .ToHashSet();
            var upperIntakes = observedIndividualMeans
                .Where(c => individualsId.Contains(c.SimulatedIndividual.Id))
                .Select(c => c.DietaryIntakePerMassUnit)
                .ToList();
            var samplingWeights = observedIndividualMeans
                .Where(c => individualsId.Contains(c.SimulatedIndividual.Id))
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            UpperPercentage = 100 - samplingWeights.Sum() / weights.Sum() * 100;
            LowPercentileValue = upperIntakes.DefaultIfEmpty(double.NaN).Min();
            HighPercentileValue = upperIntakes.DefaultIfEmpty(double.NaN).Max();
            NRecords = upperIntakes.Count;
            Summarize(upperIntakes, samplingWeights);
        }
    }
}
