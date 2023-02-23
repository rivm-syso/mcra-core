using MCRA.Utils.DateTimes;

namespace MCRA.Data.Compiled.Objects {

    public sealed class FoodOrigin {
        public Food Food { get; set; }

        public string MarketLocation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double Percentage { get; set; }
        public string OriginLocation { get; set; }

        public TimeRange Period {
            get {
                if (StartDate.HasValue && EndDate.HasValue) {
                    return new TimeRange(StartDate.Value, EndDate.Value);
                } else {
                    return null;
                }
            }
        }
    }
}
