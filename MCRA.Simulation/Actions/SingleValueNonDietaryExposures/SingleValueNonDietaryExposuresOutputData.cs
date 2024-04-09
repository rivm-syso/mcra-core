using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueNonDietaryExposuresCalculation;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {
    public class SingleValueNonDietaryExposuresOutputData : IModuleOutputData {
        public TargetUnit ExposureUnit { get; set; }
        public ICollection<ISingleValueNonDietaryExposure> SingleValueNonDietaryExposuresResults { get; set; }
        public IDictionary<string, ExposureScenario> SingleValueNonDietaryExposureScenarios { get; set; }
        public IDictionary<string, ExposureDeterminantCombination> SingleValueNonDietaryExposureDeterminantCombinations { get; set; }
        public IList<ExposureEstimate> SingleValueNonDietaryExposureEstimates { get; set; }
        public IModuleOutputData Copy() {
            return new SingleValueNonDietaryExposuresOutputData() {
                ExposureUnit = ExposureUnit,
                SingleValueNonDietaryExposuresResults = SingleValueNonDietaryExposuresResults,
                SingleValueNonDietaryExposureScenarios = SingleValueNonDietaryExposureScenarios,
                SingleValueNonDietaryExposureDeterminantCombinations = SingleValueNonDietaryExposureDeterminantCombinations,
                SingleValueNonDietaryExposureEstimates = SingleValueNonDietaryExposureEstimates
            };
        }
    }
}

