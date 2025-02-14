using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {
    public class EnvironmentalBurdenOfDiseaseOutputData : IModuleOutputData {

        public List<EnvironmentalBurdenOfDiseaseResultRecord> EnvironmentalBurdenOfDiseases { get; set; }

        public List<ExposureEffectResultRecord> ExposureEffects { get; set; }

        public IModuleOutputData Copy() {
            return new EnvironmentalBurdenOfDiseaseOutputData() {
                EnvironmentalBurdenOfDiseases = EnvironmentalBurdenOfDiseases,
                ExposureEffects = ExposureEffects
            };
        }
    }
}

