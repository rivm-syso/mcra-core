using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalTaskModelCalculation;

namespace MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation {
    public class OccupationalTaskExposureModelsCalculator {

        public static List<IOccupationalTaskExposureModel> Compute(
            ICollection<Compound> substances,
            ICollection<OccupationalTaskExposure> occupationalTaskExposures,
            List<ExposureRoute> routes,
            OccupationalExposureModelType exposureGenerationMethod,
            double selectPercentage) {
            var taskModels = new List<IOccupationalTaskExposureModel>();
            var occupationalModelFactory = new OccupationalTaskExposureModelFactory(
                exposureGenerationMethod,
                selectPercentage
            );

            var taskExposures = occupationalTaskExposures
                .Where(r => routes.Contains(r.ExposureRoute))
                .Where(r => substances.Contains(r.Substance))
                .GroupBy(r => (r.OccupationalTask, Determinants: r.Determinants(), r.ExposureRoute, r.Substance));
            foreach (var group in taskExposures) {
                var units = group.Select(r => r.Unit).Distinct().ToList();
                if (units.Count > 1) {
                    throw new NotImplementedException("Cannot combine occupational task exposure estimated with different unit types.");
                }
                var estimateTypes = group.Select(r => r.EstimateType).Distinct().ToList();
                if (estimateTypes.Count > 1) {
                    throw new NotImplementedException("Cannot combine occupational task exposure estimated with different estimate types.");
                }
                var exposureUnit = new OccupationalExposureUnit(units.FirstOrDefault(), estimateTypes.FirstOrDefault());
                var model = occupationalModelFactory
                    .Create(
                        group.Key.OccupationalTask,
                        group.Key.Determinants,
                        group.Key.ExposureRoute,
                        group.Key.Substance,
                        [.. group],
                        exposureUnit
                    );
                if (model != null) {
                    taskModels.Add(model);
                }
            }
            return taskModels;
        }
    }
}
