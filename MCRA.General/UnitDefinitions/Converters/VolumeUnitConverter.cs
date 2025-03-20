using MCRA.Utils.ExtensionMethods;

namespace MCRA.General {

    public static class VolumeUnitConverter {

        /// <summary>
        /// Gets the conversion factor to translate volume unit to the specified target unit.
        /// </summary>
        public static double GetMultiplicationFactor(
            this VolumeUnit unit,
            VolumeUnit target
        ) {
            if (unit == target
                || unit == VolumeUnit.Liter && target == VolumeUnit.Cubicmeter
                || unit == VolumeUnit.Cubicmeter && target == VolumeUnit.Liter
            ) {
                return 1;
            } else {
                throw new NotImplementedException($"Conversion from volume unit {unit.GetDisplayName()} to {unit.GetDisplayName()} not implemented.");
            }
        }
    }
}
