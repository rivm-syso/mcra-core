﻿using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.SbmlModelCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class InternalConcentrationTimeSeriesSection : SummarySection {

        public List<InternalConcentrationTimeSeriesRecord> Records { get; set; }
        public List<PbkModelParameterSummaryRecord> ParameterRecords { get; set; }

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
                .Select(r => new PbkModelParameterSummaryRecord() {
                    SubstanceCode = "SubstanceCode",
                    SubstanceName = "SubstanceName",
                    Unit = r.Unit,
                    ParameterCode = r.Id,
                    Value = r.DefaultValue.HasValue ? r.DefaultValue.Value : double.NaN,
                })
                .ToList();
            ParameterRecords = result;
        }
    }
}