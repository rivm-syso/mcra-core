namespace MCRA.General {
    public static class PointOfDepartureTypeExtensions {
        /// <summary>
        /// Converts the specified hazard dose type to its quite similar potency origin
        /// type counterpart.
        /// </summary>
        /// <param name="hazardDoseType"></param>
        /// <returns></returns>
        public static PotencyOrigin ToPotencyOrigin(this PointOfDepartureType hazardDoseType) {
            return hazardDoseType switch {
                PointOfDepartureType.Bmd => PotencyOrigin.Bmd,
                PointOfDepartureType.Noael => PotencyOrigin.Noael,
                PointOfDepartureType.Loael => PotencyOrigin.Loael,
                PointOfDepartureType.Noel => PotencyOrigin.Noel,
                PointOfDepartureType.Ld50 => PotencyOrigin.Ld50,
                PointOfDepartureType.Bmdl01 => PotencyOrigin.Bmdl01,
                PointOfDepartureType.Bmdl10 => PotencyOrigin.Bmdl10,
                _ => PotencyOrigin.Unknown,
            };
        }

        /// <summary>
        /// Converts the specified hazard dose type to its quite similar potency origin
        /// type counterpart.
        /// </summary>
        /// <param name="hazardCharacterisationType"></param>
        /// <returns></returns>
        public static PotencyOrigin ToPotencyOrigin(this HazardCharacterisationType hazardCharacterisationType) {
            return hazardCharacterisationType switch {
                HazardCharacterisationType.Unspecified => PotencyOrigin.Unknown,
                HazardCharacterisationType.Bmd => PotencyOrigin.Bmd,
                HazardCharacterisationType.Noael => PotencyOrigin.Noael,
                HazardCharacterisationType.Loael => PotencyOrigin.Loael,
                HazardCharacterisationType.Adi => PotencyOrigin.ADI,
                HazardCharacterisationType.Arfd => PotencyOrigin.ARfD,
                HazardCharacterisationType.Noel => PotencyOrigin.Noel,
                HazardCharacterisationType.Tdi => PotencyOrigin.Tdi,
                HazardCharacterisationType.Twi => PotencyOrigin.Twi,
                HazardCharacterisationType.Bmdl01 => PotencyOrigin.Bmdl01,
                HazardCharacterisationType.Bmdl10 => PotencyOrigin.Bmdl10,
                HazardCharacterisationType.Hbmgv => PotencyOrigin.Hbmgv,
                _ => PotencyOrigin.Unknown,
            };
        }
    }
}
