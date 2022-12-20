using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class NonDietarySurveyProperty {
        public string IndividualPropertyTextValue { get; set; }
        public double? IndividualPropertyDoubleValueMin { get; set; }
        public double? IndividualPropertyDoubleValueMax { get; set; }

        public IndividualProperty IndividualProperty { get; set; }
        public NonDietarySurvey NonDietarySurvey { get; set; }

        public PropertyType PropertyType {
            get {
                if (!string.IsNullOrEmpty(IndividualPropertyTextValue)) {
                    return PropertyType.Cofactor;
                } else {
                    return PropertyType.Covariable;
                }
            }
        }
    }
}
