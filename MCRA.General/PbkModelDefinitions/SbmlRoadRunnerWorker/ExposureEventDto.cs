namespace MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker {
    public sealed class ExposureEventDto {
        public string Kind { get; set; }
        public string Route { get; set; }
        public double Time { get; set; }
        public double Interval { get; set; }
        public double? TimeEnd { get; set; }
        public double Value { get; set; }
    }
}
