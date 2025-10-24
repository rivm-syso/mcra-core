using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;

namespace MCRA.Simulation.Calculators.OccupationalTaskModelCalculation {
    public sealed class OccupationalTaskExposureModelFactory {

        public OccupationalExposureModelType ExposureGenerationMethod { get; }
        public double SelectedPercentage { get; }

        public OccupationalTaskExposureModelFactory(
            OccupationalExposureModelType exposureGenerationMethod,
            double selectedPercentage
        ) {
            ExposureGenerationMethod = exposureGenerationMethod;
            SelectedPercentage = selectedPercentage;
        }

        public IOccupationalTaskExposureModel Create(
            OccupationalTask occupationalTask,
            OccupationalTaskDeterminants determinants,
            ExposureRoute route,
            Compound substance,
            List<OccupationalTaskExposure> taskExposures,
            OccupationalExposureUnit unit
        ) {
            IOccupationalTaskExposureModel model = ExposureGenerationMethod switch {
                OccupationalExposureModelType.Percentile =>
                    new OccupationalTaskExposureConstantModel(
                        occupationalTask,
                        determinants,
                        route,
                        substance,
                        taskExposures,
                        unit,
                        SelectedPercentage
                    ),
                OccupationalExposureModelType.Distribution =>
                    new OccupationalTaskExposureDistributionModel(
                        occupationalTask,
                        determinants,
                        route,
                        substance,
                        taskExposures,
                        unit
                    ),
                _ => throw new NotImplementedException(),
            };
            model.CalculateParameters();
            return model;
        }
    }
}
