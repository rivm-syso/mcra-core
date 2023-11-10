using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {

    public class PredictedHealthEffectSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double[] Percentages { get; set; }
        public double PercentageZeroIntake { get; set; }
        public string DoseResponseModelEquation { get; set; }
        public string ParameterValues { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public List<HistogramBin> PHEDistributionBins { get; set; }
        public UncertainDataPointCollection<double> PercentilesGrid { get; set; }
        public UncertainDataPointCollection<double> Percentiles { get; set; }

        public void Summarize(
            List<IndividualEffect> individualEffects,
            HealthEffectType healthEffectType,
            IHazardCharacterisationModel hazardCharacterisation,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            double[] selectedPercentiles
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            Percentages = selectedPercentiles;
            HealthEffectType = healthEffectType;
            var model = hazardCharacterisation?.TestSystemHazardCharacterisation?.DoseResponseRelation;
            DoseResponseModelEquation = model?.DoseResponseModelEquation;
            ParameterValues = model?.DoseResponseModelParameterValues;

            var predictedHealthEffects = individualEffects
                .Select(c => c.PredictedHealthEffect)
                .ToList();
            var logData = individualEffects
                .Where(c => c.PredictedHealthEffect > 0)
                .Select(c => Math.Log10(c.PredictedHealthEffect))
                .ToList();

            if (logData.Count > 0) {
                int numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(predictedHealthEffects.Count)) : 100;
                var weights = individualEffects.Where(c => c.PredictedHealthEffect > 0).Select(c => c.SamplingWeight).ToList();
                PHEDistributionBins = logData.MakeHistogramBins(weights, numberOfBins, logData.Min(), logData.Max());
            }

            PercentageZeroIntake = 100D * individualEffects.Count(c => c.HazardExposureRatio == SimulationConstants.MOE_eps) / individualEffects.Count;
            var samplingWeights = individualEffects.Select(c => c.SamplingWeight).ToList();
            PercentilesGrid = new UncertainDataPointCollection<double>();
            PercentilesGrid.XValues = GriddingFunctions.GetPlotPercentages();
            PercentilesGrid.ReferenceValues = individualEffects.Select(c => c.PredictedHealthEffect).PercentilesWithSamplingWeights(samplingWeights, PercentilesGrid.XValues);
            Percentiles = new UncertainDataPointCollection<double>();
            Percentiles.XValues = Percentages;
            Percentiles.ReferenceValues = predictedHealthEffects.PercentilesWithSamplingWeights(samplingWeights, Percentiles.XValues = Percentages);
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle (acute). Make sure to call SummarizeReferenceResults first. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="intakes">The resampled set of intakes</param>
        public void SummarizeUncertainty(List<IndividualEffect> individualEffects) {
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var predictedHealthEffects = individualEffects.Select(c => c.PredictedHealthEffect).ToList();
            PercentilesGrid.AddUncertaintyValues(predictedHealthEffects.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues));
            Percentiles.AddUncertaintyValues(predictedHealthEffects.PercentilesWithSamplingWeights(weights, Percentages));
        }
    }
}
