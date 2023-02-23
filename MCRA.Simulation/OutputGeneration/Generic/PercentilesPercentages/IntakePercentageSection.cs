using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Calculates percentages (output) for specified percentiles (input)
    /// </summary>
    public class IntakePercentageSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<IntakePercentageRecord> IntakePercentageRecords { get { return getIntakePercentageRecords(); } }

        public double UncertaintyLowerLimit { get; set; } = 2.5;

        public double UncertaintyUpperLimit { get; set; } = 97.5;

        public Compound ReferenceSubstance { get; set; }

        private UncertainDataPointCollection<double> _percentages = new UncertainDataPointCollection<double>();
        public UncertainDataPointCollection<double> Percentages { get => _percentages; set => _percentages = value; }

        private UncertainDataPointCollection<double> _numberOfPeopleExceedanceExposureLevel = new UncertainDataPointCollection<double>();
        public UncertainDataPointCollection<double> NumberOfPeopleExceedanceExposureLevel { get => _numberOfPeopleExceedanceExposureLevel; set => _numberOfPeopleExceedanceExposureLevel = value; }

        /// <summary>
        /// Summarizes the exposures for OIM, BNN, LNN0. Percentages (output) from specified percentiles (input).
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="weights"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="percentiles"></param>
        public void Summarize(
                List<double> intakes,
                List<double> weights,
                Compound referenceSubstance,
                double[] percentiles
            ) {
            ReferenceSubstance = referenceSubstance;
            Percentages = new UncertainDataPointCollection<double> {
                XValues = percentiles,
                ReferenceValues = intakes.PercentagesWithSamplingWeights(weights, percentiles)
            };

            NumberOfPeopleExceedanceExposureLevel = new UncertainDataPointCollection<double>() {
                XValues = percentiles,
                ReferenceValues = getNumberOfPeopleExceedingExposureLevel()
            };
        }

        /// <summary>
        /// Summarizes the exposures for ISUF. Percentages (output) from specified percentiles (input).
        /// </summary>
        /// <param name="isufModel"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="exposureLevels"></param>
        public void Summarize(
                ISUFModel isufModel,
                Compound referenceSubstance,
                double[] exposureLevels
            ) {
            ReferenceSubstance = referenceSubstance;
            Percentages = new UncertainDataPointCollection<double> {
                XValues = exposureLevels,
                ReferenceValues = UtilityFunctions.LinearInterpolate(isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.CumulativeProbability).ToList(), isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList(), exposureLevels.ToList())
            };
            NumberOfPeopleExceedanceExposureLevel = new UncertainDataPointCollection<double> {
                XValues = exposureLevels,
                ReferenceValues = getNumberOfPeopleExceedingExposureLevel()
            };
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle for OIM. Percentages (output) from specified percentiles (input).
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="weights"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(List<double> intakes, List<double> weights, double uncertaintyLowerBound, double uncertaintyUpperBound) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            Percentages.AddUncertaintyValues(intakes.PercentagesWithSamplingWeights(weights, Percentages.XValues.ToArray()));
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle for ISUF. Percentages (output) from specified percentiles (input).
        /// </summary>
        /// <param name="isufModel"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(ISUFModel isufModel, double uncertaintyLowerBound, double uncertaintyUpperBound) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            Percentages.AddUncertaintyValues(UtilityFunctions.LinearInterpolate(isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.CumulativeProbability).ToList(), isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList(), Percentages.XValues.ToList()));
        }

        private IEnumerable<double> getNumberOfPeopleExceedingExposureLevel() {
            var result = new List<double>();
            foreach (var item in Percentages.ReferenceValues) {
                var x = Convert.ToInt32((100 - item) * 1E6 / 100);
                result.Add(x > 0 ? x : 0);
            }
            return result;
        }
        private List<IntakePercentageRecord> getIntakePercentageRecords() {
            var counter = 0;
            var result = new List<IntakePercentageRecord>();
            if (Percentages != null) {
                for (int i = 0; i < Percentages.Count; i++) {
                    result.Add(new IntakePercentageRecord() {
                        XValues = Percentages[counter].XValue,
                        ReferenceValue = Percentages[counter].ReferenceValue / 100,
                        LowerBound = Percentages[counter].Percentile(UncertaintyLowerLimit) / 100,
                        UpperBound = Percentages[counter].Percentile(UncertaintyUpperLimit) / 100,
                        NumberOfPeopleExceedanceExposureLevel = _numberOfPeopleExceedanceExposureLevel[counter].ReferenceValue,
                        UpperBoundNumberOfPeopleExceedanceExposureLevel = (100 - Percentages[counter].Percentile(UncertaintyLowerLimit)) * 1E6 / 100,
                        LowerBoundNumberOfPeopleExceedanceExposureLevel = (100 - Percentages[counter].Percentile(UncertaintyUpperLimit)) * 1E6 / 100,
                    });
                    counter++;
                }
            }
            return result;
        }
    }
}
