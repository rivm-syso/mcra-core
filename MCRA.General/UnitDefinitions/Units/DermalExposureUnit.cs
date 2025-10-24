using MCRA.Utils.ExtensionMethods;

namespace MCRA.General {
    public class DermalExposureUnit {

        public DermalExposureUnit()
            : this(SubstanceAmountUnit.Micrograms, SurfaceAreaUnit.SquareCentimeters, TimeScaleUnit.PerDay) {
        }

        public DermalExposureUnit(
            SubstanceAmountUnit substanceAmountUnit,
            SurfaceAreaUnit surfaceAreaUnit,
            TimeScaleUnit timeScaleUnit = TimeScaleUnit.Unspecified
        ) {
            SubstanceAmountUnit = substanceAmountUnit;
            SurfaceAreaUnit = surfaceAreaUnit;
            TimeScaleUnit = timeScaleUnit;
        }

        /// <summary>
        /// The unit of the substance amounts.
        /// </summary>
        public SubstanceAmountUnit SubstanceAmountUnit { get; set; }

        /// <summary>
        /// The (skin) surface area unit (e.g., cm2).
        /// </summary>
        public SurfaceAreaUnit SurfaceAreaUnit { get; set; }

        /// <summary>
        /// The time scale. E.g., per day, peak (acute), long term (chronic).
        /// </summary>
        public TimeScaleUnit TimeScaleUnit { get; set; }

        /// <summary>
        /// Identification code of this target.
        /// </summary>
        public string GetDisplayName() {
            return $"{SubstanceAmountUnit.GetShortDisplayName()}" +
                $"/{SurfaceAreaUnit.GetShortDisplayName()}" +
                $"/{TimeScaleUnit.GetShortDisplayName()}";
        }

        /// <summary>
        /// Identification code of this target.
        /// </summary>
        public string GetShortDisplayName() {
            return $"{SubstanceAmountUnit.GetShortDisplayName()}" +
                $"/{SurfaceAreaUnit.GetShortDisplayName()}" +
                $"/{TimeScaleUnit.GetShortDisplayName()}";
        }
    }
}
