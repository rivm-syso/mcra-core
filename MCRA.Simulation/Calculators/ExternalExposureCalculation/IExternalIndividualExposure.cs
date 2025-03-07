using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public interface IExternalIndividualExposure {
        SimulatedIndividual SimulatedIndividual { get; }
        Dictionary<ExposurePath, List<IIntakePerCompound>> ExposuresPerPath { get; set; }
        bool HasPositives(ExposureRoute route, Compound substance);
        List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }
    }
}
