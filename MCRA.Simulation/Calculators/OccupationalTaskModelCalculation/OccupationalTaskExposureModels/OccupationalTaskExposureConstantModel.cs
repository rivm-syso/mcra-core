using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.OccupationalTaskModelCalculation {
    public sealed class OccupationalTaskExposureConstantModel : OccupationalTaskExposureModelBase {

        OccupationalTaskExposure ConstantRecord;

        public double SelectedPercentage { get; set; }

        public OccupationalTaskExposureConstantModel(
            OccupationalTask task,
            OccupationalTaskDeterminants determinants,
            ExposureRoute route,
            Compound substance,
            List<OccupationalTaskExposure> taskExposures,
            OccupationalExposureUnit unit,
            double selectedPercentage
        ) : base(task, determinants, route, substance, taskExposures, unit) {
            SelectedPercentage = selectedPercentage;
        }

        public override void CalculateParameters() {
            ConstantRecord = TaskExposures.FirstOrDefault(c => c.Percentage == SelectedPercentage)
                ?? throw new Exception($"Selected percentage {SelectedPercentage}% is not available in task exposures table.");
        }

        public override double DrawFromDistribution(IRandom random) {
            return ConstantRecord.Value;
        }

        public override double GetNominal() {
            return ConstantRecord.Value;
        }

        public override string GetModelType() {
            return "Constant";
        }

        public override string GetModelDescription() {
            return $"Constant model based on p{SelectedPercentage}";
        }
    }
}
