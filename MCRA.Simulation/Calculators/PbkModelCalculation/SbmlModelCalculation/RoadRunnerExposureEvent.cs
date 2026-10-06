namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation;

public sealed class RoadRunnerExposureEvent {
    public string Kind { get; set; }
    public string Route { get; set; }
    public double Time { get; set; }
    public double Interval { get; set; }
    public double? TimeEnd { get; set; }
    public double Value { get; set; }
}
