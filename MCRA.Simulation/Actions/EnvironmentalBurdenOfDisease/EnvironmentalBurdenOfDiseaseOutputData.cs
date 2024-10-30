using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {
    public class EnvironmentalBurdenOfDiseaseOutputData : IModuleOutputData {

        public List<EnvironmentalBurdenOfDiseaseResultRecord> AttributableEbds { get; set; }

        public List<ExposureEffectResultRecord> ExposureEffects { get; set; }

        public IModuleOutputData Copy() {
            return new EnvironmentalBurdenOfDiseaseOutputData() {
                AttributableEbds = AttributableEbds,
                ExposureEffects = ExposureEffects
            };
        }
    }
}

