using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {
    public class ExternalExposureDistributionBase : SummarySection, IIntakeDistributionSection {

        public override bool SaveTemporaryData => true;

        protected readonly double _upperWhisker = 95;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];

        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public List<HistogramBin> IntakeDistributionBinsCoExposure { get; set; }
        public UncertainDataPointCollection<double> Percentiles { get; set; }
        public int TotalNumberOfExposures { get; set; }
        public double PercentageZeroIntake { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }

        public void Summarize(
            HashSet<int> coExposureIds,
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double[] percentages,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit,
            bool isPerPerson
        ) {
            UncertaintyLowerLimit = uncertaintyLowerLimit;
            UncertaintyUpperLimit = uncertaintyUpperLimit;
            Percentiles = [];
            var min = 0d;
            var max = 0d;
            var numberOfBins = 100;

            var externalExposures = externalIndividualDayExposures
                .Select(id => (
                    TotalExternalexposure: id.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                    SamplingWeight: id.SimulatedIndividual.SamplingWeight,
                    id.SimulatedIndividualDayId
                ))
                .ToList();

            var positives = externalExposures
                .Where(r => r.TotalExternalexposure > 0)
                .ToList();

            if (positives.Count != 0) {
                var logData = positives.Select(c => Math.Log10(c.TotalExternalexposure)).ToList();
                var weights = positives.Select(id => id.SamplingWeight).ToList();
                min = logData.Min();
                max = logData.Max();
                TotalNumberOfExposures = externalIndividualDayExposures.Count;
                numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(logData.Count)) : 100;
                IntakeDistributionBins = logData.MakeHistogramBins(weights, numberOfBins, min, max);
                PercentageZeroIntake = 100 - logData.Count / (double)TotalNumberOfExposures * 100;
            } else {
                IntakeDistributionBins = null;
                PercentageZeroIntake = 100D;
            }

            // Summarize the exposures based on a grid defined by the percentages array
            if (percentages != null && percentages.Length > 0) {
                Percentiles.XValues = percentages;
                var weights = externalExposures
                    .Select(c => c.SamplingWeight)
                    .ToList();
                Percentiles.ReferenceValues = externalExposures
                    .Select(i => i.TotalExternalexposure)
                    .PercentilesWithSamplingWeights(weights, percentages);
            }

            if (coExposureIds != null && coExposureIds.Count > 0) {
                var logCoExposureIntakes = externalExposures
                   .Where(i => coExposureIds.Contains(i.SimulatedIndividualDayId))
                   .Select(i => Math.Log10(i.TotalExternalexposure))
                   .ToList();

                if (logCoExposureIntakes.Count != 0) {
                    var avg = logCoExposureIntakes.Average();
                    var weights = externalExposures
                        .Where(i => coExposureIds.Contains(i.SimulatedIndividualDayId))
                        .Select(i => i.SamplingWeight)
                        .ToList();

                    IntakeDistributionBinsCoExposure = logCoExposureIntakes.MakeHistogramBins(weights, numberOfBins, min, max);
                }
            }
        }
    }
}
