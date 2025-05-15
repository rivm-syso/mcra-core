using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Individual(int id) {
        private string _name;
        private readonly ICollection<IndividualPropertyValue> _individualPropertyValues = [];

        public int Id { get; } = id;

        public string Code { get; set; }

        public string Name {
            get => string.IsNullOrEmpty(_name) ? Code : _name;
            set => _name = value;
        }
        public string Description { get; set; }

        public double SamplingWeight { get; set; } = 1D;

        public double BodyWeight { get; set; }

        public int NumberOfDaysInSurvey { get; set; }

        public double Covariable { get; set; }

        public string Cofactor { get; set; }

        public string CodeFoodSurvey { get; set; }

        public IEnumerable<IndividualPropertyValue> IndividualPropertyValues => _individualPropertyValues;

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Id:000} {Code}";
        }

        public IDictionary<string, IndividualDay> IndividualDays { get; set; } =
            new Dictionary<string, IndividualDay>(StringComparer.OrdinalIgnoreCase);

        public void SetPropertyValue(
            IndividualProperty property,
            string textValue = null,
            double? doubleValue = null
        ) => SetPropertyValue(new() {
            IndividualProperty = property,
            TextValue = textValue,
            DoubleValue = doubleValue
        });

        public void SetPropertyValue(IndividualPropertyValue value) {
            _individualPropertyValues.Add(value);
            var property = value.IndividualProperty;
            if (property.IsAgeProperty) {
                Age = value.DoubleValue;
            } else if (property.IsHeightProperty) {
                Height = value.DoubleValue;
            } else if (property.IsSexProperty) {
                Gender = GenderTypeConverter.FromString(value.TextValue, GenderType.Undefined, true);
            } else if (property.IsBsaProperty) {
                BodySurfaceArea = value.DoubleValue;
            }
        }

        public double? GetDoubleValue(IndividualProperty property) => GetPropertyValue(property)?.DoubleValue;

        public string GetTextValue(IndividualProperty property) => GetPropertyValue(property)?.TextValue;

        public IndividualPropertyValue GetPropertyValue(IndividualProperty property) =>
            _individualPropertyValues.FirstOrDefault(v => v.IndividualProperty.Code.Equals(property.Code,
                StringComparison.OrdinalIgnoreCase));

        public double? Age { get; private set; }

        public double? Height { get; private set; }

        public GenderType Gender { get; private set; }

        public double? BodySurfaceArea { get; private set; }
    }
}
