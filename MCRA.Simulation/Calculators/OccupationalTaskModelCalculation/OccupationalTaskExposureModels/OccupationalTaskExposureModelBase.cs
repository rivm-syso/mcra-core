using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.OccupationalTaskModelCalculation {
    public abstract class OccupationalTaskExposureModelBase : IOccupationalTaskExposureModel {

        public OccupationalTask Task { get; init; }
        public OccupationalTaskDeterminants Determinants { get; init; }
        public Compound Substance { get; init; }
        public List<OccupationalTaskExposure> TaskExposures { get; init; }
        public ExposureRoute Route { get; init; }
        public OccupationalExposureUnit Unit { get; init; }

        public OccupationalTaskExposureModelBase(
            OccupationalTask task,
            OccupationalTaskDeterminants determinants,
            ExposureRoute route,
            Compound substance,
            List<OccupationalTaskExposure> taskExposures,
            OccupationalExposureUnit unit
        ) {
            Task = task;
            Determinants = determinants;
            Route = route;
            Substance = substance;
            TaskExposures = taskExposures;
            Unit = unit;
        }

        public abstract void CalculateParameters();
        public abstract double DrawFromDistribution(IRandom random);
        public abstract double GetNominal();
        public abstract string GetModelType();
        public abstract string GetModelDescription();
    }
}
