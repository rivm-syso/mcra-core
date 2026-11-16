using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.PbkModelCalculation;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels.KineticConversionFactorModelCalculation {
    public class KineticConversionFactorCalculationResult {

        public ExposureRoute Route { get; set; }

        public Compound Substance { get; set; }

        public ExposureUnitTriple ExposureUnit { get; set; }

        public List<(double Dose, PbkSimulationOutput SimulationOutput)> PbkSimulationOutputs { get; set; }

        public List<IKineticConversionFactorModel> ConversionFactors { get; set; }


    }
}
