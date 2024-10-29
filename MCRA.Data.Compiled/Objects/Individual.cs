using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Individual {
        private string _name;
        public Individual(int id) {
            Id = id;
            IndividualPropertyValues = new HashSet<IndividualPropertyValue>();
            IndividualDays = new Dictionary<string, IndividualDay>(StringComparer.OrdinalIgnoreCase);
        }

        public int Id { get; private set; }

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

        public double SamplingWeight { get; set; } = 1D;

        public double BodyWeight { get; set; }

        public int NumberOfDaysInSurvey { get; set; }

        public double Covariable { get; set; }

        public string Cofactor { get; set; }

        public string CodeFoodSurvey { get; set; }

        public ICollection<IndividualPropertyValue> IndividualPropertyValues { get; set; }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Id:000} {Code}";
        }

        public IDictionary<string, IndividualDay> IndividualDays { get; set; }

        public double? GetAge() {
            var age = IndividualPropertyValues?
                .FirstOrDefault(c => c.IndividualProperty.IsAgeProperty())?
                .DoubleValue;
            return age;
        }

        public GenderType GetGender() {
            var genderString = IndividualPropertyValues?
                .FirstOrDefault(c => c.IndividualProperty.IsSexProperty())?
                .TextValue;
            var sex = GenderTypeConverter.FromString(genderString, GenderType.Undefined);
            return sex;
        }

        public double? GetBsa() {
            var bsa = IndividualPropertyValues?
                .FirstOrDefault(c => c.IndividualProperty.IsBsaProperty())?
                .DoubleValue;
            if (!bsa.HasValue) {
                // Calculation of BSA using the Mosteller formula.
                // TODO: this is not the right place for computing the BSA.
                // Also, this formula does not use imputed bodyweights.
                var height = GetHeight();
                var weight = BodyWeight;
                if (height.HasValue) {
                    bsa = Math.Sqrt(height.Value * weight / 3600);
                }
            }
            return bsa;
        }

        public double? GetHeight() {
            var height = IndividualPropertyValues?
                .FirstOrDefault(c => c.IndividualProperty.IsHeightProperty())?
                .DoubleValue;
            return height;
        }
    }
}
