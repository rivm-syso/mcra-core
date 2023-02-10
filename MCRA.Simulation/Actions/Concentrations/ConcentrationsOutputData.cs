
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;

namespace MCRA.Simulation.Actions.Concentrations {
    public class ConcentrationsOutputData : IModuleOutputData {
        public ILookup<Food, FoodSample> FoodSamples { get; set; }
        public IDictionary<Food, List<ISampleOrigin>> SampleOriginInfos { get; set; }
        public ICollection<SampleCompoundCollection> MeasuredSubstanceSampleCollections { get; set; }
        public ICollection<SampleCompoundCollection> ActiveSubstanceSampleCollections { get; set; }
        public ICollection<FoodSubstanceExtrapolationCandidates> ExtrapolationCandidates { get; set; }
        public IModuleOutputData Copy() {
            return new ConcentrationsOutputData() {
                FoodSamples = FoodSamples,
                SampleOriginInfos = SampleOriginInfos,
                MeasuredSubstanceSampleCollections = MeasuredSubstanceSampleCollections,
                ActiveSubstanceSampleCollections= ActiveSubstanceSampleCollections,
                ExtrapolationCandidates = ExtrapolationCandidates,
            };
        }
    }
}

