namespace MCRA.General.Annotations {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class IsCalculationSettingsAttribute : Attribute {

        public bool IsCalculationSettings { get; private set; }

        public IsCalculationSettingsAttribute() {
            IsCalculationSettings = true;
        }

        public IsCalculationSettingsAttribute(bool isCalculationSettings) {
            IsCalculationSettings = isCalculationSettings;
        }
    }
}
