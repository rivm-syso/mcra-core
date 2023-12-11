namespace MCRA.Simulation.OutputGeneration {
    public class HCSubgroupSubstancePlotRecords {
        public double Value { get; set; }
        //TODO, not yet implemented, may be useful for age dependent plot
        public double ValueUncLower { get; set; }
        //TODO, not yet implemented, may be useful for age dependent plot
        public double ValueUncUpper { get; set; }
        public string Unit { get; set; }
        public string SubstanceName { get; set; }
        public List<HCSubgroupPlotRecord> PlotRecords { get; set; }
    }
}
