namespace MCRA.Data.Compiled.Objects {
    public sealed class NonDietaryExposureSet
    {
        public NonDietaryExposureSet() {
            NonDietaryExposures = new HashSet<NonDietaryExposure>();
        }

        public string Code { get; set; }
        public string IndividualCode { get; set; }
        public NonDietarySurvey NonDietarySurvey { get; set; }
        public ICollection<NonDietaryExposure> NonDietaryExposures { get; set; }

        public bool IsUncertaintySet() {
            return !string.IsNullOrEmpty(Code);
        }
    }
}
