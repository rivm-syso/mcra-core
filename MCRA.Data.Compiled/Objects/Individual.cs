﻿using MCRA.General;

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
            if (IndividualPropertyValues?.Any() ?? false) {
                var age = IndividualPropertyValues
                    .FirstOrDefault(c => c.IndividualProperty.Name == "Age")?.DoubleValue;
                return age;
            }
            return null;
        }

        public GenderType GetGender() {
            if (IndividualPropertyValues?.Any() ?? false) {
                var gender = IndividualPropertyValues
                    .FirstOrDefault(c => c.IndividualProperty.Name == "Gender")?.TextValue;
                return GenderTypeConverter.FromString(gender);
            }
            return GenderType.Undefined;
        }
    }
}
