using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardDistributionSection : SummarySection {
        public override bool SaveTemporaryData => true;

        private readonly double _eps = 10E7D;

        public TargetUnit TargetUnit { get; set; }
        public double CriticalEffectDoseAnimal { get; set; }
        public double CriticalEffectDoseHuman { get; set; }
        public double DegreesOfFreedom { get; set; }
        public double GeometricStandardDeviation { get; set; }
        public double LowerVariationFactor { get; set; }
        public double UpperVariationFactor { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double PercentageZeroIntake { get; set; }
        public UncertainDataPointCollection<double> PercentilesGrid { get; set; }
        public List<HistogramBin> CEDDistributionBins { get; set; }

        public void Summarize(
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel hazardCharacterisation,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesConversionModels
        ) {
            TargetUnit = new TargetUnit(hazardCharacterisation.Target, hazardCharacterisation.DoseUnit);
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var marginsOfExposure = individualEffects.Select(c => c.HazardExposureRatio).ToList();
            PercentageZeroIntake = 100D * marginsOfExposure.Count(c => c == _eps) / marginsOfExposure.Count;
            CriticalEffectDoseAnimal = hazardCharacterisation.Value / hazardCharacterisation.CombinedAssessmentFactor;
            CriticalEffectDoseHuman = hazardCharacterisation.Value;
            GeometricStandardDeviation = hazardCharacterisation?.GeometricStandardDeviation ?? double.NaN;

            // Drilldown information from intra species
            var intraSpeciesFactorModel = intraSpeciesConversionModels.Get(hazardCharacterisation.Effect, hazardCharacterisation.Substance);
            DegreesOfFreedom = intraSpeciesFactorModel?.DegreesOfFreedom ?? double.NaN;
            LowerVariationFactor = intraSpeciesFactorModel?.IntraSpeciesFactor?.LowerVariationFactor ?? double.NaN;
            UpperVariationFactor = intraSpeciesFactorModel?.IntraSpeciesFactor?.UpperVariationFactor ?? double.NaN;

            var logData = individualEffects.Select(c => Math.Log10(c.CriticalEffectDose)).ToList();
            if (logData.Count > 0) {
                int numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(logData.Count)) : 100;
                var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
                CEDDistributionBins = logData.MakeHistogramBins(weights, numberOfBins, logData.Min(), logData.Max());
            }

            // Summarize the exposures for based on a grid defined by the percentages array
            var samplingWeights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var individualCriticalEffectsDoses = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            PercentilesGrid = new UncertainDataPointCollection<double>();
            PercentilesGrid.XValues = GriddingFunctions.GetPlotPercentages();
            PercentilesGrid.ReferenceValues = individualCriticalEffectsDoses.PercentilesWithSamplingWeights(samplingWeights, PercentilesGrid.XValues);
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle (acute). Make sure to call SummarizeReferenceResults first. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="intakes">The resampled set of intakes</param>
        public void SummarizeUncertainty(List<IndividualEffect> individualEffects) {
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var individualCriticalEffectsDoses = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            PercentilesGrid.AddUncertaintyValues(individualCriticalEffectsDoses.PercentilesWithSamplingWeights(weights, PercentilesGrid.XValues));
        }
    }
}
