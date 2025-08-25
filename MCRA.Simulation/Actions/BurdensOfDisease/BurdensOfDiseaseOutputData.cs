using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.BurdensOfDisease {
    public class BurdensOfDiseaseOutputData : IModuleOutputData {
        public IList<BurdenOfDisease> BurdensOfDisease { get; set; }
        public IList<BodIndicatorConversion> BodIndicatorConversions { get; set; }

        public IModuleOutputData Copy() {
            return new BurdensOfDiseaseOutputData() {
                BurdensOfDisease = BurdensOfDisease,
                BodIndicatorConversions = BodIndicatorConversions
            };
        }
    }
}
