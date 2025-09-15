using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.HbmSingleValueExposures {
    public class HbmSingleValueExposuresOutputData : IModuleOutputData {
        public ConcentrationUnit HbmConcentrationUnit { get; set; }
        public ICollection<HbmSingleValueExposureSet> HbmSingleValueExposureSets { get; set; }
        public IModuleOutputData Copy() {
            return new HbmSingleValueExposuresOutputData() {
                HbmConcentrationUnit = HbmConcentrationUnit,
                HbmSingleValueExposureSets = HbmSingleValueExposureSets
            };
        }
    }
}

