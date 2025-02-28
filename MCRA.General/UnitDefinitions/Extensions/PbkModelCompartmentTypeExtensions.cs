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
        /// Get priority of compartment to serve as input compartment for the specified route.
        /// </summary>
        public static int GetPriority(this PbkModelCompartmentType compartmentType, ExposureRoute route) {
            return compartmentType switch {
                PbkModelCompartmentType.AlveolarAir => route == ExposureRoute.Inhalation ? 4 : -1,
                PbkModelCompartmentType.Lung => route == ExposureRoute.Inhalation ? 3 : -1,
                PbkModelCompartmentType.ArterialBlood => route == ExposureRoute.Inhalation ? 2 : -1,
                PbkModelCompartmentType.ArterialPlasma => route == ExposureRoute.Inhalation ? 1 : -1,
                PbkModelCompartmentType.Gut => route == ExposureRoute.Oral ? 2 : -1,
                PbkModelCompartmentType.StratumCorneumExposedSkin => route == ExposureRoute.Dermal ? 2 : -1,
                PbkModelCompartmentType.Skin => route == ExposureRoute.Dermal ? 1 : -1,
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
                    or PbkModelCompartmentType.StratumCorneumExposedSkin
                    => ExposureRoute.Dermal,
                _ => ExposureRoute.Undefined,
            };
        }
    }
}
