using MCRA.General;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.ExposureEventsGeneration {
    public class RepeatingExposureEvent : IExposureEvent {

        /// <summary>
        /// Exposure route of the event.
        /// </summary>
        public ExposureRoute Route { get; set; }

        /// <summary>
        /// Starting time of the first event of the repeating event.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// End time of the repeating event.
        /// </summary>
        public double? TimeEnd { get; set; }

        /// <summary>
        /// Event interval / time between two events.
        /// </summary>
        public double Interval { get; set; }

        /// <summary>
        /// Exposure value.
        /// </summary>
        public double Value { get; set; }

        public List<SingleExposureEvent> Expand(double timeEnd) {
            var result = new List<SingleExposureEvent>();
            timeEnd = TimeEnd.HasValue && TimeEnd.Value < timeEnd
                ? TimeEnd.Value : timeEnd;
            for (var time = Time; time < timeEnd; time += Interval) {
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
