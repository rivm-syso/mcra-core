using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Calculates percentiles (output) for specified percentages (input)
    /// </summary>
    public class HazardPercentileSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public ReferenceDoseRecord Reference { get; set; }
        public UncertainDataPoint<double> MeanHazardCharacterisation { get; set; }
        public UncertainDataPointCollection<double> Percentiles { get; set; }

        public void Summarize(
            List<IndividualEffect> individualEffects,
            double[] percentages,
            IHazardCharacterisationModel referenceDose
        ) {
            var hazards = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            Reference = ReferenceDoseRecord.FromHazardCharacterisation(referenceDose);
            Percentiles = new UncertainDataPointCollection<double> {
                XValues = percentages,
                ReferenceValues = hazards.PercentilesWithSamplingWeights(weights, percentages)
            };
            MeanHazardCharacterisation = new UncertainDataPoint<double>() { ReferenceValue = hazards.Average(weights) };
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle for  Risk (Margin of Exposure
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
            if (Percentiles == null) {
                return new List<HazardPercentileRecord>();
            }
            var counter = 0;
            var result = new List<HazardPercentileRecord>();
            for (int i = 0; i < Percentiles.Count; i++) {
                result.Add(new HazardPercentileRecord() {
                    XValues = Percentiles[counter].XValue / 100,
                    ReferenceValue = Percentiles[counter].ReferenceValue,
                });
                counter++;
            }
            for (int i = 0; i < Percentiles.Count; i++) {
                result[i].LowerBound = Percentiles[i].Percentile(UncertaintyLowerLimit);
                result[i].UpperBound = Percentiles[i].Percentile(UncertaintyUpperLimit);
                result[i].Median = Percentiles[i].MedianUncertainty;
            }
            return result;
        }
    }
}
