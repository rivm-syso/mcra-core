namespace MCRA.General {
    public static class PbkModelCompartmentTypeExtensions {

        /// <summary>
        /// Returns the biological matrix associated with the specified compartment
        /// type.
        /// </summary>
        public static BiologicalMatrix GetBiologicalMatrix(this PbkModelCompartmentType compartmentType) {
            var result = BiologicalMatrixConverter
                .FromString(compartmentType.ToString(), BiologicalMatrix.Undefined, allowInvalidString: true);
            return result;
        }

        /// <summary>
        /// Get priority of compartment to serve as input compartment feeding exposures at systemic level.
        /// </summary>
        public static int GetSystemicInputPriority(this PbkModelCompartmentType compartmentType) {
            return compartmentType switch {
                PbkModelCompartmentType.ArterialBlood => 100,
                PbkModelCompartmentType.VenousBlood => 95,
                PbkModelCompartmentType.ArterialPlasma => 90,
                PbkModelCompartmentType.VenousPlasma => 85,
                PbkModelCompartmentType.Blood => 80,
                PbkModelCompartmentType.BloodPlasma => 70,
                _ => -1,
            };
        }

        /// <summary>
        /// Get priority of compartment to serve as input compartment for the specified route.
        /// </summary>
        public static int GetExposureRouteInputPriority(this PbkModelCompartmentType compartmentType, ExposureRoute route) {
            return route switch {
                ExposureRoute.Oral => compartmentType switch {
                    PbkModelCompartmentType.Gut => 2,
                    _ => -1,
                },
                ExposureRoute.Dermal => compartmentType switch {
                    PbkModelCompartmentType.StratumCorneumExposedSkin => 3,
                    PbkModelCompartmentType.ExposedSkin => 2,
                    PbkModelCompartmentType.Skin => 1,
                    _ => -1,
                },
                ExposureRoute.Inhalation => compartmentType switch {
                    PbkModelCompartmentType.AlveolarAir => 4,
                    PbkModelCompartmentType.Lung => 3,
                    PbkModelCompartmentType.ArterialBlood => 2,
                    PbkModelCompartmentType.ArterialPlasma => 1,
                    _ => -1,
                },
                _ => -1,
            };
        }

        public static ExposureRoute GetExposureRoute(this PbkModelCompartmentType compartmentType) {
            return compartmentType switch {
                PbkModelCompartmentType.AlveolarAir
                    or PbkModelCompartmentType.Lung
                    or PbkModelCompartmentType.ArterialBlood
                    or PbkModelCompartmentType.ArterialPlasma
                    => ExposureRoute.Inhalation,
                PbkModelCompartmentType.Gut => ExposureRoute.Oral,
                PbkModelCompartmentType.Skin
                    or PbkModelCompartmentType.ExposedSkin
                    or PbkModelCompartmentType.StratumCorneumExposedSkin
                    => ExposureRoute.Dermal,
                _ => ExposureRoute.Undefined,
            };
        }
    }
}
