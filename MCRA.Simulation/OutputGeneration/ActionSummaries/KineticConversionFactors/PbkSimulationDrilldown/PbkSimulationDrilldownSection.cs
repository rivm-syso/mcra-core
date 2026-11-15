using MCRA.Simulation.Calculators.PbkModelCalculation;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {

    public class PbkSimulationDrilldownSection : SummarySection {

        public bool IsLifetime { get; set; }
        public int NumberOfDaysSkipped { get; set; }

        public List<PbkSimulationDrilldownRecord> Records { get; set; }

        public void Summarize(
            ICollection<PbkSimulationOutput> pbkSimulationOutputs,
            bool isLifeTime
        ) {
            var records = new List<PbkSimulationDrilldownRecord>();
            var ix = 0;
            foreach (var output in pbkSimulationOutputs) {
                ix++;
                foreach (var item in output.SubstanceTargetLevelTimeSeries) {
                    var record = new PbkSimulationDrilldownRecord() {
                        SimulationId = $"Simulation {ix}",
                        EndpointCode = $"{item.Substance.Code}_{item.TargetUnit.Target}",
                        EndpointName = $"{item.Substance.Name} in {item.TargetUnit.Target.GetDisplayName()}",
                        Unit = item.TargetUnit.GetShortDisplayName(DisplayOption.UnitOnly),
                        OutputTimeSeries = [.. item.Exposures.Select(r => (r.Time, r.Exposure))]
                    };
                    records.Add(record);
                }
                if (output.BodyWeightTimeSeries != null) {
                    var record = new PbkSimulationDrilldownRecord() {
                        SimulationId = $"Simulation {ix}",
                        EndpointCode = $"BW",
                        EndpointName = $"Bodyweight",
                        Unit = "kg",
                        OutputTimeSeries = [.. output.BodyWeightTimeSeries]
                    };
                    records.Add(record);
                }
            }
            Records = records;
            NumberOfDaysSkipped = 10;
            IsLifetime = isLifeTime;
        }
    }
}
