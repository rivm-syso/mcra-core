namespace MCRA.General {
    public static class PointOfDepartureTypeExtensions {
        /// <summary>
        /// Converts the specified hazard dose type to its quite similar potency origin
        /// type counterpart.
        /// </summary>
        public static PotencyOrigin ToPotencyOrigin(this PointOfDepartureType podType) {
            return podType switch {
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

        public static double GetExpressionTypeConversionFactor(this PointOfDepartureType podType, PointOfDepartureType sourceType) {
            return podType switch {
                PointOfDepartureType.Bmd => toBmdFactor(sourceType),
                PointOfDepartureType.Bmdl01 or PointOfDepartureType.Bmdl10 
                    => throw new Exception(message: $"No conversion from {sourceType} to {podType}"),
                PointOfDepartureType.Noael => toNoaelFactor(sourceType),
                PointOfDepartureType.Loael => throw new Exception(message: $"No conversion from {sourceType} to LOAEL"),
                PointOfDepartureType.Unspecified => 1D,
                _ => throw new Exception(message: $"No conversion from {sourceType} to {podType}"),
            };
        }

        private static double toBmdFactor(PointOfDepartureType sourceType) {
            return sourceType switch {
                PointOfDepartureType.Bmd => 1D,
                PointOfDepartureType.Bmdl01 or PointOfDepartureType.Bmdl10 
                    => throw new Exception(message: $"No conversion from {sourceType} to benchmark dose"),
                PointOfDepartureType.Loael => toNoaelFactor(PointOfDepartureType.Loael) * toBmdFactor(PointOfDepartureType.Noael),
                PointOfDepartureType.Noael => 3D,
                _ => throw new Exception(message: $"No conversion from {sourceType} to benchmark dose"),
            };
        }

        private static double toNoaelFactor(PointOfDepartureType sourceType) {
            return sourceType switch {
                PointOfDepartureType.Bmd => 1D / 3,
                PointOfDepartureType.Bmdl01 or PointOfDepartureType.Bmdl10 
                    => throw new Exception(message: $"No conversion from {sourceType} to NOAEL"),
                PointOfDepartureType.Noael => 1,
                PointOfDepartureType.Loael => 1D / 3,
                _ => throw new Exception(message: $"No conversion from {sourceType} to NOAEL"),
            };
        }
    }
}
