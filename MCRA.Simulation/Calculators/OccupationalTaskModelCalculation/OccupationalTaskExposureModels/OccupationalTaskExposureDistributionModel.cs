using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.OccupationalTaskModelCalculation {
    public sealed class OccupationalTaskExposureDistributionModel : OccupationalTaskExposureModelBase {

        public Distribution Distribution { get; private set; }

        public OccupationalTaskExposureDistributionModel(
            OccupationalTask task,
            OccupationalTaskDeterminants determinants,
            ExposureRoute route,
            Compound substance,
            List<OccupationalTaskExposure> taskExposures,
            OccupationalExposureUnit unit
        ) : base(task, determinants, route, substance, taskExposures, unit) {
        }

        public override void CalculateParameters() {
            //TODO implement rrisksdistributions package
            var mu = TaskExposures.Average(r => Math.Log(r.Value));
            var sigma = 1;
            Distribution = new LogNormalDistribution(mu, sigma);
        }

        public override double DrawFromDistribution(IRandom random) {
            var draw = Distribution.Draw(random);
            return draw;
        }

        public override double GetNominal() {
            throw new NotImplementedException();
        }

        public override string GetModelType() {
            return "Constant";
        }

        public override string GetModelDescription() {
            var percentiles = TaskExposures.Select(r => r.Percentage.ToString("G4")).ToList();
            var percentilesStr = string.Join(", ", percentiles);
            return $"Log-normal distribution model based on [{percentilesStr}] percentiles";
        }
    }
}
