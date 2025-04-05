using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ExposureEvent {
    public class RepeatingExposureEvent : IExposureEvent {
        public ExposureRoute Route { get; set; }
        public double TimeStart { get; set; }
        public double? TimeEnd { get; set; }
        public double Interval { get; set; }
        public double Value { get; set; }

        public List<SingleExposureEvent> Expand(double timeEnd) {
            var result = new List<SingleExposureEvent>();
            timeEnd = TimeEnd.HasValue && TimeEnd.Value < timeEnd
                ? TimeEnd.Value : timeEnd;
            for (double time = TimeStart; time < timeEnd; time += Interval) {
                var record = new SingleExposureEvent() {
                    Route = Route,
                    Time = time,
                    Value = Value
                };
                result.Add(record);
            }
            return result;
        }
    }
}
