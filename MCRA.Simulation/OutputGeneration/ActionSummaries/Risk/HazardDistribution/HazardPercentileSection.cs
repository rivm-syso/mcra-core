using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Calculates percentiles (output) for specified percentages (input)
    /// </summary>
    public class HazardPercentileSection : PercentileBootstrapSectionBase<IntakePercentileRiskBootstrapRecord> {
        public override bool SaveTemporaryData => true;

        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public ReferenceDoseRecord Reference { get; set; }
        public UncertainDataPoint<double> MeanHazardCharacterisation { get; set; }

        public void Summarize(
            List<IndividualEffect> individualEffects,
            double[] percentages,
            IHazardCharacterisationModel referenceDose
        ) {
            var hazards = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            Reference = referenceDose != null ? new ReferenceDoseRecord(referenceDose.Substance) : null;
            TargetUnit = new TargetUnit(referenceDose.Target, referenceDose.DoseUnit);
            Percentiles = new UncertainDataPointCollection<double> {
                XValues = percentages,
                ReferenceValues = hazards.PercentilesWithSamplingWeights(weights, percentages)
            };
            MeanHazardCharacterisation = new UncertainDataPoint<double>() { ReferenceValue = hazards.Average(weights) };
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle for Risk
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        public void SummarizeUncertainty(List<IndividualEffect> individualEffects, double lowerBound, double upperBound) {
            UncertaintyLowerLimit = lowerBound;
            UncertaintyUpperLimit = upperBound;
            var hazards = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            Percentiles.AddUncertaintyValues(hazards.PercentilesWithSamplingWeights(weights, Percentiles.XValues.ToArray()));
            MeanHazardCharacterisation.UncertainValues.Add(hazards.Average(weights));
        }

        public List<HazardPercentileRecord> GetHazardPercentileRecords() {
            var result = Percentiles?
                .Select(p => new HazardPercentileRecord {
                    XValues = p.XValue / 100,
                    ReferenceValue = p.ReferenceValue,
                    LowerBound = p.Percentile(UncertaintyLowerLimit),
                    UpperBound = p.Percentile(UncertaintyUpperLimit),
                    Median = p.MedianUncertainty
                }).ToList() ?? [];

            return result;
        }
    }
}
