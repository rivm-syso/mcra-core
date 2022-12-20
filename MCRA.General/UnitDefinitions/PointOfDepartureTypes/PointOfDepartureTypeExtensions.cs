namespace MCRA.General {
    public static class PointOfDepartureTypeExtensions {
        /// <summary>
        /// Converts the specified hazard dose type to its quite similar potency origin
        /// type counterpart.
        /// </summary>
        /// <param name="hazardDoseType"></param>
        /// <returns></returns>
        public static PotencyOrigin ToPotencyOrigin(this PointOfDepartureType hazardDoseType) {
            switch (hazardDoseType) {
                case PointOfDepartureType.Bmd:
                    return PotencyOrigin.Bmd;
                case PointOfDepartureType.Noael:
                    return PotencyOrigin.Noael;
                case PointOfDepartureType.Loael:
                    return PotencyOrigin.Loael;
                case PointOfDepartureType.Noel:
                    return PotencyOrigin.Noel;
                case PointOfDepartureType.Ld50:
                    return PotencyOrigin.Ld50;
                case PointOfDepartureType.Bmdl01:
                    return PotencyOrigin.Bmdl01;
                case PointOfDepartureType.Bmdl10:
                    return PotencyOrigin.Bmdl10;
                default:
                    return PotencyOrigin.Unknown;
            }
        }

        /// <summary>
        /// Converts the specified hazard dose type to its quite similar potency origin
        /// type counterpart.
        /// </summary>
        /// <param name="hazardCharacterisationType"></param>
        /// <returns></returns>
        public static PotencyOrigin ToPotencyOrigin(this HazardCharacterisationType hazardCharacterisationType) {
            switch (hazardCharacterisationType) {
                case HazardCharacterisationType.Unspecified:
                    return PotencyOrigin.Unknown;
                case HazardCharacterisationType.Bmd:
                    return PotencyOrigin.Bmd;
                case HazardCharacterisationType.Noael:
                    return PotencyOrigin.Noael;
                case HazardCharacterisationType.Loael:
                    return PotencyOrigin.Loael;
                case HazardCharacterisationType.Adi:
                    return PotencyOrigin.ADI;
                case HazardCharacterisationType.Arfd:
                    return PotencyOrigin.ARfD;
                case HazardCharacterisationType.Noel:
                    return PotencyOrigin.Noel;
                case HazardCharacterisationType.Tdi:
                    return PotencyOrigin.Tdi;
                case HazardCharacterisationType.Twi:
                    return PotencyOrigin.Twi;
                case HazardCharacterisationType.Bmdl01:
                    return PotencyOrigin.Bmdl01;
                case HazardCharacterisationType.Bmdl10:
                    return PotencyOrigin.Bmdl10;
                default:
                    return PotencyOrigin.Unknown;
            }
        }
    }
}
