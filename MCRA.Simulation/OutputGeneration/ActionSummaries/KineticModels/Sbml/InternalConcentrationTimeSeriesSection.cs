using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class InternalConcentrationTimeSeriesSection : SummarySection {

        public List<InternalConcentrationTimeSeriesRecord> Records { get; set; }
        public List<ParameterRecord> ParameterRecords { get; set; }

        public void Summarize(List<InternalConcentrationTimeSeries> concentrations) {
            var result = concentrations
                .Select(r => new InternalConcentrationTimeSeriesRecord() {
                    Compartment = r.Id,
                    Values = r.Values,
                    TimeScale = r.TimeScale,
                    TimeFrequency = r.TimeFrequency,
                    Unit = r.Unit,
                })
                .ToList();
            Records = result;
        }

        public void Summarize(List<KineticModelParameterDefinition> parameters) {
            var result = parameters
                .Select(r => new ParameterRecord() {
                    Code = "SubstanceCode",
                    Name = "SubstanceName",
                    Unit = r.Unit,
                    Parameter = r.Id,
                    Value = r.DefaultValue.HasValue ? r.DefaultValue.Value : double.NaN,
                })
                .ToList();
            ParameterRecords = result;
        }
    }
}
