using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    /// <summary>
    /// Calculates percentiles (output) for specified percentages (input)
    /// </summary>
    public class IntakePercentileSection : SummarySection {

        public override bool SaveTemporaryData => true;

        public double UncertaintyLowerLimit { get; set; } = 2.5;
        public double UncertaintyUpperLimit { get; set; } = 97.5;
        public Compound ReferenceSubstance { get; set; }

        public UncertainDataPointCollection<double> MeanOfExposure { get; set; } = new UncertainDataPointCollection<double>();
        public UncertainDataPointCollection<double> Percentiles { get; set; } = new UncertainDataPointCollection<double>();
        public List<IntakePercentileRecord> IntakePercentileRecords { get { return getIntakePercentileRecords(); } }

        /// <summary>
        /// Summarizes the exposures for OIM,BBN,LNN0.
        /// Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="weights"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="percentages"></param>
        public void Summarize(
            List<double> intakes,
            List<double> weights,
            Compound referenceSubstance,
            double[] percentages
        ) {
            ReferenceSubstance = referenceSubstance;
            Percentiles.XValues = percentages;
            Percentiles.ReferenceValues = intakes.PercentilesWithSamplingWeights(weights, percentages);
            MeanOfExposure.XValues = new double[] { 0 };
            MeanOfExposure.ReferenceValues = new List<double> { intakes.Average(weights) };
        }

        /// <summary>
        /// Summarizes the exposures for ISUF. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="isufModel"></param>
        /// <param name="selectedPercentiles"></param>
        public void Summarize(
            ISUFModel isufModel,
            Compound referenceSubstance,
            double[] selectedPercentiles
        ) {
            ReferenceSubstance = referenceSubstance;
            var xIdev = new List<double>();
            foreach (var p in selectedPercentiles) {
                xIdev.Add(Math.Sqrt(isufModel.TransformationResult.VarianceBetweenUnit) * NormalDistribution.InvCDF(0, 1, p / 100));
            }
            Percentiles.XValues = selectedPercentiles;
            Percentiles.ReferenceValues = UtilityFunctions.LinearInterpolate(
                isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList(),
                isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.Deviate).ToList(),
                xIdev
            );
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle for BNN,LNN,LNN0. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="weights"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
            List<double> intakes,
            List<double> weights,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            Percentiles.AddUncertaintyValues(intakes.PercentilesWithSamplingWeights(weights, Percentiles.XValues.ToArray()));
            MeanOfExposure.AddUncertaintyValues(new List<double> { intakes.Average(weights) });
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle for ISUF. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="isufModel"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
                ISUFModel isufModel,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            var xIdev = new List<double>();
            foreach (var p in Percentiles.XValues) {
                xIdev.Add(Math.Sqrt(isufModel.TransformationResult.VarianceBetweenUnit) * NormalDistribution.InvCDF(0, 1, p / 100));
            }
            var uncertaintyValues = UtilityFunctions.LinearInterpolate(
                isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList(),
                isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.Deviate).ToList(),
                xIdev
            );
            Percentiles.AddUncertaintyValues(uncertaintyValues);
        }

        public List<IntakePercentileBootstrapRecord> GetIntakePercentileBootstrapRecords() {
            var result = new List<IntakePercentileBootstrapRecord>();
            for (int i = 0; i < Percentiles.Count; i++) {
                result.Add(new IntakePercentileBootstrapRecord() {
                    Percentile = Percentiles[i].XValue / 100,
                    Exposure = Percentiles[i].ReferenceValue,
                });
            }

            for (int i = 0; i < Percentiles.Count; i++) {
                for (int j = 0; j < Percentiles[i].UncertainValues.Count; j++) {
                    result.Add(new IntakePercentileBootstrapRecord() {
                        Bootstrap = j + 1,
                        Percentile = Percentiles[i].XValue / 100,
                        Exposure = Percentiles[i].UncertainValues[j],
                    });
                }
            }
            return result;
        }

        private List<IntakePercentileRecord> getIntakePercentileRecords() {
            var counter = 0;
            var result = new List<IntakePercentileRecord>();
            for (int i = 0; i < Percentiles.Count; i++) {
                result.Add(new IntakePercentileRecord() {
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
