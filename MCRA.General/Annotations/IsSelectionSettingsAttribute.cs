using System;

namespace MCRA.General.Annotations {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class IsSelectionSettingsAttribute : Attribute {

        public bool IsSelectionSettings { get; private set; }

        public IsSelectionSettingsAttribute() {
            IsSelectionSettings = true;
        }

        public IsSelectionSettingsAttribute(bool isSelectionSettings) {
            IsSelectionSettings = isSelectionSettings;
        }
    }
}
