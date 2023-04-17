
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SingleValueConcentrations {
    public class SingleValueConcentrationsOutputData : IModuleOutputData {
        public ICollection<ConcentrationSingleValue> SingleValueConcentrations { get; set; }
        public IDictionary<(Food Food, Compound Substance), SingleValueConcentrationModel> MeasuredSubstanceSingleValueConcentrations { get; set; }
        public IDictionary<(Food Food, Compound Substance), SingleValueConcentrationModel> ActiveSubstanceSingleValueConcentrations { get; set; }
        public ConcentrationUnit SingleValueConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new SingleValueConcentrationsOutputData() {
                SingleValueConcentrations = SingleValueConcentrations,
                ActiveSubstanceSingleValueConcentrations = ActiveSubstanceSingleValueConcentrations,
                MeasuredSubstanceSingleValueConcentrations = MeasuredSubstanceSingleValueConcentrations,
                SingleValueConcentrationUnit = SingleValueConcentrationUnit
            };
        }
    }
}

