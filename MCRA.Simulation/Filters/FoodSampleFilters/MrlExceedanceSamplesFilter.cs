using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Filters.FoodSampleFilters {
    public sealed class MrlExceedanceSamplesFilter : FoodSampleFilterBase {

        /// <summary>
        /// The maximum residue limits against which the substance concentrations are to be tested.
        /// </summary>
        private readonly IDictionary<Food, IDictionary<Compound, ConcentrationLimit>> _maximumResidueLimits;

        /// <summary>
        /// Fraction of maximum residue limit.
        /// </summary>
        private readonly double _fractionMrl = 1D;

        /// <summary>
        /// Initializes a new <see cref="FoodSampleFilterBase"/> instance.
        /// </summary>
        /// <param name="maximumConcentrationLimits"></param>
        /// <param name="fractionMrl"></param>
        public MrlExceedanceSamplesFilter(ICollection<ConcentrationLimit> maximumConcentrationLimits, double fractionMrl) : base() {
            _maximumResidueLimits = new Dictionary<Food, IDictionary<Compound, ConcentrationLimit>>();
            if (maximumConcentrationLimits != null) {
                foreach (var mrl in maximumConcentrationLimits) {
                    if (!_maximumResidueLimits.TryGetValue(mrl.Food, out var mrlsOfFood)) {
                        mrlsOfFood = new Dictionary<Compound, ConcentrationLimit>();
                        _maximumResidueLimits[mrl.Food] = mrlsOfFood;
                    }
                    mrlsOfFood[mrl.Compound] = mrl;
                }
            }
            _fractionMrl = fractionMrl;
        }

        /// <summary>
        /// Implements <see cref="FoodSampleFilterBase.Passes(FoodSample)"/>.
        /// Passes the filter when all sample concentrations are smaller than the MRL threshold (defined by the fraction times the MRL value). 
        /// If there is only one concentration value that exceeds the threshold, then the sample should be filtered out.
        /// TODO in documentation. Note for samples with more than one analytical method (sample analysis),
        /// a sample is considered to exceed the MRL threshold whenever there is one measured positive residues exceeding the threshold.
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override bool Passes(FoodSample foodSample) {
            if (_maximumResidueLimits.TryGetValue(foodSample.Food, out var mrlsOfFood)) {
                foreach (var sampleAnalysis in foodSample.SampleAnalyses) {
                    foreach (var concentration in sampleAnalysis.Concentrations) {
                        if (mrlsOfFood.TryGetValue(concentration.Key, out var mrl)) {
                            var substance = concentration.Key;
                            if (sampleAnalysis.AnalyticalMethod.AnalyticalMethodCompounds.TryGetValue(substance, out var amc)) {
                                var concentrationUnit = amc.GetConcentrationUnit();
                                var concentrationCorrection = concentrationUnit.GetConcentrationUnitMultiplier(mrl.ConcentrationUnit);
                                if (concentrationCorrection * concentration.Value.Concentration > _fractionMrl * mrl.Limit) {
                                    return false;
                                }
                            }
                        }
                    }
                }
                var substanceConcentrations = foodSample.SampleAnalyses.SelectMany(c => c.Concentrations.Keys).Distinct().ToList();
            }
            return true;
        }
    }
}
