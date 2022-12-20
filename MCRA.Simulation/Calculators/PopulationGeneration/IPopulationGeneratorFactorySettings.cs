using MCRA.General;

namespace MCRA.Simulation.Calculators.PopulationGeneration {
    public interface IPopulationGeneratorFactorySettings {
        ExposureType ExposureType { get; }
        bool IsSurveySampling { get; }
        int NumberOfSimulatedIndividualDays { get; }
    }
}
