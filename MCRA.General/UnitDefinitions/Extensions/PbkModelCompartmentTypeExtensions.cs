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
            switch (compartmentType) {
                case PbkModelCompartmentType.AlveolarAir:
                    return route == ExposureRoute.Inhalation ? 1 : -1;
                case PbkModelCompartmentType.ArterialBlood:
                    return route == ExposureRoute.Inhalation ? 2 : -1;
                case PbkModelCompartmentType.Gut:
                    return route == ExposureRoute.Oral ? 2 : -1;
                case PbkModelCompartmentType.Skin:
                    return route == ExposureRoute.Dermal ? 1 : -1;
                case PbkModelCompartmentType.StratumCorneumExposedSkin:
                    return route == ExposureRoute.Dermal ? 1 : -1;
                default:
                    return -1;
            }
        }

        public static ExposureRoute GetExposureRoute(this PbkModelCompartmentType compartmentType) {
            switch (compartmentType) {
                case PbkModelCompartmentType.AlveolarAir:
                case PbkModelCompartmentType.ArterialBlood:
                    return ExposureRoute.Inhalation;
                case PbkModelCompartmentType.Gut:
                    return ExposureRoute.Oral;
                case PbkModelCompartmentType.Skin:
                case PbkModelCompartmentType.StratumCorneumExposedSkin:
                    return ExposureRoute.Dermal;
                default:
                    return ExposureRoute.Undefined;
            }
        }
    }
}
