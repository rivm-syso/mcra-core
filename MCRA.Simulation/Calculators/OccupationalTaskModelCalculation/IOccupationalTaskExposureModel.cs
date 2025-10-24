using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.OccupationalTaskModelCalculation {
    public interface IOccupationalTaskExposureModel {
        public OccupationalTask Task { get; init; }
        public OccupationalTaskDeterminants Determinants { get; init; }
        public Compound Substance { get; init; }
        public ExposureRoute Route { get; init; }
        public OccupationalExposureUnit Unit { get; init; }

        void CalculateParameters();
        double DrawFromDistribution(IRandom random);
        double GetNominal();
        public string GetModelType();
        public string GetModelDescription();
    }
}
