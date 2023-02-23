using MCRA.Utils.DateTimes;

namespace MCRA.Data.Compiled.Objects {
    /// <summary>
    /// AgriculturalUse
    /// </summary>
    public class OccurrencePattern {
        private string _name;
        public OccurrencePattern() {
            Compounds = new HashSet<Compound>();
        }

        public string Code { get; set; }
        public string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return Code;
                }
                return _name;
            }
            set {
                _name = value;
            }
        }
        public string Description { get; set; }
        public string Location { get; set; }

        /// <summary>
        /// OccurrenceFraction is always used in code instead
        /// of the PercentageCropTreated which is read from the database
        /// </summary>
        public double OccurrenceFraction { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Food Food { get; set; }
        public ICollection<Compound> Compounds { get; set; }

        public TimeRange Period {
            get {
                if (StartDate != null && EndDate != null) {
                    return new TimeRange((DateTime)StartDate, (DateTime)EndDate);
                } else {
                    return null;
                }
            }
        }


        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code} - {Food}";
        }
    }
}
