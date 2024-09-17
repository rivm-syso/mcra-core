
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels;

namespace MCRA.Simulation.Actions.ExposureBiomarkerConversions {
    public class ExposureBiomarkerConversionsOutputData : IModuleOutputData {

        public ICollection<ExposureBiomarkerConversion> ExposureBiomarkerConversions { get; set; }
        public ICollection<IExposureBiomarkerConversionModel> ExposureBiomarkerConversionModels { get; set; }

        public IModuleOutputData Copy() {
            return new ExposureBiomarkerConversionsOutputData() {
                ExposureBiomarkerConversions = ExposureBiomarkerConversions,
                ExposureBiomarkerConversionModels = ExposureBiomarkerConversionModels
            };
        }
    }
}

