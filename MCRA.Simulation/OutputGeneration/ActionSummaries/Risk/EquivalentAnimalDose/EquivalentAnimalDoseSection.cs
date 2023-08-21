using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public class EquivalentAnimalDoseSection : SummarySection {
        public override bool SaveTemporaryData => true;

        private readonly double _eps = 10E7D;

        public double PercentageZeroIntake { get; set; }
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public string DoseResponseModelEquation { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double[] Percentages { get; set; }
        public ReferenceDoseRecord Reference { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public UncertainDataPointCollection<double> PercentilesGrid { get; set; }
        public UncertainDataPointCollection<double> Percentiles { get; set; }
        public List<HistogramBin> EADDistributionBins { get; set; }

        public void Summarize(
                List<IndividualEffect> individualEffects,
                HealthEffectType healthEffectType,
                IHazardCharacterisationModel referenceDose,
                Compound referenceSubstance,
                IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound,
                double[] selectedPercentiles
            ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            Percentages = selectedPercentiles;
            HealthEffectType = healthEffectType;
            Reference = ReferenceDoseRecord.FromHazardCharacterisation(referenceDose);
            var hazardDoseResponseModel = hazardCharacterisations[referenceSubstance];
            DoseResponseModelEquation = hazardDoseResponseModel?.TestSystemHazardCharacterisation?.DoseResponseRelation?.DoseResponseModelEquation;
            PercentageZeroIntake = 100D * individualEffects.Count(c => c.HazardExposureRatio == _eps) / individualEffects.Count;
            var equivalentAnimalDoses = individualEffects.Select(c => c.EquivalentTestSystemDose).ToList();

            var logData = individualEffects.Where(c => c.EquivalentTestSystemDose > 0).Select(c => Math.Log10(c.EquivalentTestSystemDose)).ToList();
            if (logData.Count > 0) {
                int numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(equivalentAnimalDoses.Count)) : 100;
                var weights = individualEffects.Where(c => c.EquivalentTestSystemDose > 0).Select(c => c.SamplingWeight).ToList();
                EADDistributionBins = logData.MakeHistogramBins(weights, numberOfBins, logData.Min(), logData.Max());
            }
            // Summarize the exposures for based on a grid defined by the percentages array
            var samplingWeights = individualEffects.Select(c => c.SamplingWeight).ToList();
            PercentilesGrid = new UncertainDataPointCollection<double>();
            PercentilesGrid.XValues = GriddingFunctions.GetPlotPercentages();
            PercentilesGrid.ReferenceValues = individualEffects.Select(c => c.EquivalentTestSystemDose).PercentilesWithSamplingWeights(samplingWeights, PercentilesGrid.XValues);

            var totalSamplingWeights = samplingWeights.Sum();
            Mean = individualEffects.Sum(c => c.EquivalentTestSystemDose * c.SamplingWeight) / totalSamplingWeights;
            var sum = 0D;
            foreach (var item in individualEffects) {
                sum += Math.Pow((item.EquivalentTestSystemDose * item.SamplingWeight - Mean * item.SamplingWeight), 2);
            }
            StandardDeviation = Math.Sqrt(sum / totalSamplingWeights);
            Percentiles = new UncertainDataPointCollection<double>();
            Percentiles.XValues = Percentages;
            Percentiles.ReferenceValues = equivalentAnimalDoses.PercentilesWithSamplingWeights(samplingWeights, Percentiles.XValues = Percentages);
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle (acute). Make sure to call SummarizeReferenceResults first. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="individualEffects"></param>
        public void SummarizeUncertainty(List<IndividualEffect> individualEffects) {
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var equivalentAnimalDoses = individualEffects.Select(c => c.EquivalentTestSystemDose).ToList();
            PercentilesGrid.AddUncertaintyValues(equivalentAnimalDoses.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues));
            Percentiles.AddUncertaintyValues(equivalentAnimalDoses.PercentilesWithSamplingWeights(weights, Percentages));
        }
    }
}
