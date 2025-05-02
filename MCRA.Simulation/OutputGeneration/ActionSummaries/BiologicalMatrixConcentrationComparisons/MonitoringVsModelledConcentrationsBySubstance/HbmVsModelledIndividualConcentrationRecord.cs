namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmVsModelledIndividualConcentrationRecord {
        public string Individual { get; set; }
        public double ModelledExposure { get; set; }
        public double MonitoringConcentration { get; set; }

        public bool BothPositive => ModelledExposure > 0 && MonitoringConcentration > 0;
    }
}
