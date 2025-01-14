using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class DietaryDistributionSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public List<HistogramBin> IntakeDistributionBinsCoExposure { get; set; }
        public int TotalNumberOfIntakes { get; set; }
        public double PercentageZeroIntake { get; set; }

        //public ReferenceDoseRecord Reference { get; set; }

        public double UncertaintyLowerlimit { get; set; }
        public double UncertaintyUpperlimit { get; set; }

        protected UncertainDataPointCollection<double> _percentiles = [];
        public UncertainDataPointCollection<double> Percentiles { get => _percentiles; set => _percentiles = value; }

        public void Summarize(
            HashSet<int> coExposureIds,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double[] percentages,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            UncertaintyLowerlimit = uncertaintyLowerLimit;
            UncertaintyUpperlimit = uncertaintyUpperLimit;
            TotalNumberOfIntakes = dietaryIndividualDayIntakes.Count;
            var min = 0d;
            var max = 0d;
            int numberOfBins = 100;

            var exposures = dietaryIndividualDayIntakes
                .Select(r => (
                    IsCoExposure: coExposureIds?.Contains(r.SimulatedIndividualDayId) ?? true,
                    SamplingWeight: r.SimulatedIndividual.SamplingWeight,
                    TotalExposure: r.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)
                ))
                .ToList();

            var positiveExposures = exposures.Where(r => r.TotalExposure > 0).ToList();
            if (positiveExposures.Any()) {
                var logData = positiveExposures.Select(i => Math.Log10(i.TotalExposure)).ToList();
                min = logData.Min();
                max = logData.Max();
                numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(logData.Count)) : 100;
                var weights = positiveExposures.Select(i => i.SamplingWeight).ToList();
                IntakeDistributionBins = logData.MakeHistogramBins(weights, numberOfBins, min, max);
            }
            PercentageZeroIntake = 100 - positiveExposures.Count / (double)TotalNumberOfIntakes * 100;

            // Summarize the exposures for based on a grid defined by the percentages array
            if (percentages != null && percentages.Length > 0) {
                _percentiles.XValues = percentages;
                var weights = exposures.Select(c => c.SamplingWeight).ToList();
                _percentiles.ReferenceValues = exposures
                    .Select(i => i.TotalExposure)
                    .PercentilesWithSamplingWeights(weights, percentages);
            }

            if (coExposureIds != null && coExposureIds.Count > 0) {
                var result = positiveExposures.Where(i => i.IsCoExposure).ToList();
                if (result.Any()) {
                    var logCoExposureIntakes = result.Select(i => Math.Log10(i.TotalExposure)).ToList();
                    var weights = result.Select(i => i.SamplingWeight).ToList();
                    IntakeDistributionBinsCoExposure = logCoExposureIntakes.MakeHistogramBins(weights, numberOfBins, min, max);
                }
            }
        }
    }
}
