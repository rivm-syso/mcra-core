using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;

namespace MCRA.Simulation.Actions.KineticModels {
    public class KineticModelsOutputData : IModuleOutputData {
        public ICollection<KineticModelInstance> KineticModelInstances { get; set; }
        public ICollection<KineticAbsorptionFactor> KineticAbsorptionFactors { get; set; }
        public ICollection<KineticConversionFactor> KineticConversionFactors { get; set; }
        public ICollection<KineticConversionFactorModelBase> KineticConversionFactorModels { get; set; }
        public IDictionary<(ExposurePathType, Compound), double> AbsorptionFactors { get; set; }
        public IModuleOutputData Copy() {
            return new KineticModelsOutputData() {
                KineticModelInstances = KineticModelInstances,
                KineticAbsorptionFactors = KineticAbsorptionFactors,
                AbsorptionFactors = AbsorptionFactors,
                KineticConversionFactors = KineticConversionFactors,
                KineticConversionFactorModels = KineticConversionFactorModels
            };
        }
    }
}

