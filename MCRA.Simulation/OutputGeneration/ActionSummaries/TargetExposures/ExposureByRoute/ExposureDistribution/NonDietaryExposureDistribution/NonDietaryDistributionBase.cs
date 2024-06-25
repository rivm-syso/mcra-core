using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class NonDietaryDistributionBase : ActionSummarySectionBase, IIntakeDistributionSection {
        public override bool SaveTemporaryData => true;
        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public List<HistogramBin> IntakeDistributionBinsCoExposure { get; set; }

        public int TotalNumberOfIntakes { get; set; }
        public double PercentageZeroIntake { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        protected UncertainDataPointCollection<double> _percentiles = new();
        public UncertainDataPointCollection<double> Percentiles { get => _percentiles; set => _percentiles = value; }

        public void Summarize(
                HashSet<int> coExposureIds,
                ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                double[] percentages,
                double uncertaintyLowerLimit,
                double uncertaintyUpperLimit,
                bool isPerPerson
            ) {
            UncertaintyLowerLimit = uncertaintyLowerLimit;
            UncertaintyUpperLimit = uncertaintyUpperLimit;
            Percentiles = new UncertainDataPointCollection<double>();
            var min = 0d;
            var max = 0d;
            var numberOfBins = 100;

            var nonDietaryIntakes = nonDietaryIndividualDayIntakes
                .Select(id => (
                    TotalNonDietaryIntake: id.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson),
                    SamplingWeight: id.IndividualSamplingWeight,
                    SimulatedIndividualDayId: id.SimulatedIndividualDayId
                ))
                .ToList();

            var positives = nonDietaryIntakes
                .Where(r => r.TotalNonDietaryIntake > 0)
                .ToList();

            if (positives.Any()) {
                var logData = positives.Select(c => Math.Log10(c.TotalNonDietaryIntake)).ToList();
                var weights = positives.Select(id => id.SamplingWeight).ToList();
                min = logData.Min();
                max = logData.Max();
                TotalNumberOfIntakes = nonDietaryIndividualDayIntakes.Count;
                numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(logData.Count)) : 100;
                IntakeDistributionBins = logData.MakeHistogramBins(weights, numberOfBins, min, max);
                PercentageZeroIntake = 100 - logData.Count / (double)TotalNumberOfIntakes * 100;
            } else {
                IntakeDistributionBins = null;
                PercentageZeroIntake = 100D;
            }

            // Summarize the exposures based on a grid defined by the percentages array
            if (percentages != null && percentages.Length > 0) {
                Percentiles.XValues = percentages;
                var weights = nonDietaryIntakes.Select(c => c.SamplingWeight).ToList();
                Percentiles.ReferenceValues = nonDietaryIntakes.Select(i => i.TotalNonDietaryIntake).PercentilesWithSamplingWeights(weights, percentages);
            }

            if (coExposureIds != null && coExposureIds.Count > 0) {
                var logCoExposureIntakes = nonDietaryIntakes
                   .Where(i => coExposureIds.Contains(i.SimulatedIndividualDayId))
                   .Select(i => Math.Log10(i.TotalNonDietaryIntake))
                   .ToList();

                if (logCoExposureIntakes.Any()) {
                    var avg = logCoExposureIntakes.Average();
                    var weights = nonDietaryIntakes
                        .Where(i => coExposureIds.Contains(i.SimulatedIndividualDayId))
                        .Select(i => i.SamplingWeight)
                        .ToList();

                    IntakeDistributionBinsCoExposure = logCoExposureIntakes.MakeHistogramBins(weights, numberOfBins, min, max);
                }
            }
        }
    }
}
