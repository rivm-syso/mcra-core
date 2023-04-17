
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;

namespace MCRA.Simulation.Actions.Concentrations {
    public class ConcentrationsOutputData : IModuleOutputData {
        public ILookup<Food, FoodSample> FoodSamples { get; set; }
        public ICollection<Food> MeasuredFoods { get; set; }
        public ICollection<Compound> MeasuredSubstances { get; set; }
        public ICollection<Compound> ModelledSubstances { get; set; }
        public IDictionary<Food, List<ISampleOrigin>> SampleOriginInfos { get; set; }
        public IDictionary<Food, SampleCompoundCollection> MeasuredSubstanceSampleCollections { get; set; }
        public IDictionary<Food, SampleCompoundCollection> ActiveSubstanceSampleCollections { get; set; }
        public ICollection<FoodSubstanceExtrapolationCandidates> ExtrapolationCandidates { get; set; }
        public ConcentrationUnit ConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new ConcentrationsOutputData() {
                FoodSamples = FoodSamples,
                MeasuredSubstances = MeasuredSubstances,
                ModelledSubstances = ModelledSubstances,
                SampleOriginInfos = SampleOriginInfos,
                MeasuredSubstanceSampleCollections = MeasuredSubstanceSampleCollections,
                ActiveSubstanceSampleCollections = ActiveSubstanceSampleCollections,
                ExtrapolationCandidates = ExtrapolationCandidates,
                ConcentrationUnit = ConcentrationUnit
            };
        }
    }
}

