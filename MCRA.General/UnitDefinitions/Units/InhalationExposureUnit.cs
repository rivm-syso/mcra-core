using MCRA.Utils.ExtensionMethods;

namespace MCRA.General {
    public class InhalationExposureUnit {

        public InhalationExposureUnit()
            : this(SubstanceAmountUnit.Micrograms, VolumeUnit.Cubicmeter) {
        }

        public InhalationExposureUnit(
            SubstanceAmountUnit substanceAmountUnit,
            VolumeUnit volumeUnit
        ) {
            SubstanceAmountUnit = substanceAmountUnit;
            VolumeUnit = volumeUnit;
        }

        /// <summary>
        /// The unit of the substance amounts.
        /// </summary>
        public SubstanceAmountUnit SubstanceAmountUnit { get; set; }

        /// <summary>
        /// The (air) volume unit (e.g., m3).
        /// </summary>
        public VolumeUnit VolumeUnit { get; set; }

        /// <summary>
        /// Identification code of this target.
        /// </summary>
        public string GetDisplayName() {
            return $"{SubstanceAmountUnit.GetShortDisplayName()}" +
                $"/{VolumeUnit.GetShortDisplayName()}";
        }

        /// <summary>
        /// Identification code of this target.
        /// </summary>
        public string GetShortDisplayName() {
            return $"{SubstanceAmountUnit.GetShortDisplayName()}" +
                $"/{VolumeUnit.GetShortDisplayName()}";
        }
    }
}
