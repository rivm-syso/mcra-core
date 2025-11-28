using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels.CounterFactualValueModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctions {

    public class ExposureResponseModel : IExposureResponseModel {

        public ExposureResponseModel() {}

        /// <summary>
        /// Exposure response function definition.
        /// </summary>
        public ExposureResponseFunction ExposureResponseFunction { get; init; }

        /// <summary>
        /// Counterfactual value (uncertainty distribution) model.
        /// </summary>
        public ICounterFactualValueModel CounterFactualValueModel { get; init; }

        /// <summary>
        /// ERF function model.
        /// </summary>
        public IExposureResponseFunctionModel ErfModel { get; init; }

        /// <summary>
        /// ERF subgroup function model.
        /// </summary>
        public List<(double? Upper, IExposureResponseFunctionModel ErfModel)> ErfSubGroupModels { get; init; }

        public string Code => ExposureResponseFunction.Code;

        public string Name => ExposureResponseFunction.Name;

        public TargetUnit TargetUnit => ExposureResponseFunction.TargetUnit;

        public Compound Substance => ExposureResponseFunction.Substance;

        public Effect Effect => ExposureResponseFunction.Effect;

        public EffectMetric EffectMetric => ExposureResponseFunction.EffectMetric;

        public ExposureResponseType ExposureResponseType => ExposureResponseFunction.ExposureResponseType;

        public bool HasErfSubGroups => ErfSubGroupModels.Count > 0;

        public PopulationCharacteristicType PopulationCharacteristic => ExposureResponseFunction.PopulationCharacteristic;

        public double? EffectThresholdLower => ExposureResponseFunction.EffectThresholdLower;

        public double? EffectThresholdUpper => ExposureResponseFunction.EffectThresholdUpper;

        public List<double> SubGroupLevels {
            get {
                return [.. ErfSubGroupModels
                    .OrderBy(r => r.Upper ?? double.PositiveInfinity)
                    .Select(r => r.Upper ?? double.NaN)];
            }
        }

        /// <summary>
        /// Evaluates the ERF model for exposure x.
        /// </summary>
        public double Compute(double exposure, bool useErfBins) {
            if (exposure <= CounterFactualValueModel.GetCounterFactualValue()) {
                return EffectMetric == EffectMetric.NegativeShift
                    || EffectMetric == EffectMetric.PositiveShift ? 0D : 1D;
            }

            var counterFactualValue = CounterFactualValueModel.GetCounterFactualValue();
            if (HasErfSubGroups && useErfBins) {
                var erfSubGroups = ErfSubGroupModels
                    .OrderBy(r => r.Upper ?? double.PositiveInfinity)
                    .ToList();
                var i = 0;
                while (i < erfSubGroups.Count && erfSubGroups[i].Upper < exposure) {
                    i++;
                }
                if (i == erfSubGroups.Count) {
                    throw new Exception($"Subgroup not defined for {ExposureResponseFunction.Code} for exposure of {exposure}.");
                }
                return erfSubGroups[i].ErfModel.Compute(exposure, counterFactualValue);
            } else {
                return ErfModel.Compute(exposure, counterFactualValue);
            }
        }

        /// <summary>
        /// Gets the counter factual value from the internal counter factual value model.
        /// </summary>
        /// <returns></returns>
        public double GetCounterFactualValue() {
            return CounterFactualValueModel.GetCounterFactualValue();
        }

        public void ResampleCounterFactualValue(IRandom random) {
            CounterFactualValueModel.ResampleModelParameters(random);
        }

        public void ResampleExposureResponseFunction(IRandom random) {
            var seed = random.Next();
            var rnd = new McraRandomGenerator(seed);
            ErfModel.Resample(rnd);
            foreach (var sgModel in ErfSubGroupModels) {
                random.Reset();
                sgModel.ErfModel.Resample(random);
            }
        }
    }
}

