using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels.KineticConversionFactorModelCalculation;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {

    public class PbkSimulationDrilldownSection : SummarySection {

        public bool IsLifetime { get; set; }
        public int NumberOfDaysSkipped { get; set; }
        public string DoseUnit { get; set; }

        public List<PbkSimulationDrilldownRecord> Records { get; set; }

        public void Summarize(
            KineticConversionFactorCalculationResult resultRecord,
            ExposureUnitTriple exposureUnit,
            bool isLifeTime,
            int numberOfDaysSkipped
        ) {
            var ix = 0;
            var records = new List<PbkSimulationDrilldownRecord>();
            foreach (var output in resultRecord.PbkSimulationOutputs) {
                ix++;
                foreach (var item in output.SimulationOutput.SubstanceTargetLevelTimeSeries) {
                    var record = new PbkSimulationDrilldownRecord() {
                        SimulationId = $"{ix}",
                        Dose = output.Dose,
                        DoseRoute = resultRecord.Route,
                        SubstanceCode = resultRecord.Substance.Code,
                        SubstanceName = resultRecord.Substance.Name,
                        EndpointCode = $"{item.Substance.Code}_{item.TargetUnit.Target}",
                        EndpointName = $"{item.Substance.Name} in {item.TargetUnit.Target.GetDisplayName()}",
                        EndpointUnit = item.TargetUnit.GetShortDisplayName(DisplayOption.UnitOnly),
                        OutputTimeSeries = [.. item.Exposures.Select(r => (r.Time, r.Exposure))]
                    };
                    records.Add(record);
                }
                if (output.SimulationOutput.BodyWeightTimeSeries != null) {
                    var record = new PbkSimulationDrilldownRecord() {
                        SimulationId = $"{ix}",
                        Dose = output.Dose,
                        DoseRoute = resultRecord.Route,
                        SubstanceCode = resultRecord.Substance.Code,
                        SubstanceName = resultRecord.Substance.Name,
                        EndpointCode = $"BW",
                        EndpointName = $"Bodyweight",
                        EndpointUnit = "kg",
                        OutputTimeSeries = [.. output.SimulationOutput.BodyWeightTimeSeries]
                    };
                    records.Add(record);
                }
            }
            Records = records;
            NumberOfDaysSkipped = numberOfDaysSkipped;
            IsLifetime = isLifeTime;
            DoseUnit = exposureUnit.GetShortDisplayName();
        }
    }
}
