using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelCompartmentExtensions {

        public static PbkModelCompartmentType GetCompartmentType(this SbmlModelCompartment compartment) {
            var types = compartment.BqmIsResources?
                .Select(r => PbkModelCompartmentTypeConverter
                    .FromUri(
                        r,
                        defaultType: PbkModelCompartmentType.Undefined,
                        allowInvalidString: true
                    ))
                .Where(r => r != PbkModelCompartmentType.Undefined)
                .Distinct()
                .ToList();
            if (types?.Count > 1) {
                throw new Exception($"Incorrect annotation for compartment {compartment.Id}: compartment is linked to multiple types.");
            } else if (types?.Count == 1) {
                return types.First();
            }
            return PbkModelCompartmentType.Undefined;
        }

        public static BiologicalMatrix GetBiologicalMatrix(this SbmlModelCompartment compartment) {
            var result = compartment.GetCompartmentType().GetBiologicalMatrix();
            return result;
        }

        public static int GetPriority(this SbmlModelCompartment compartment, ExposureRoute exposureRoute) {
            return compartment.GetCompartmentType().GetPriority(exposureRoute);
        }

        public static ExposureRoute GetExposureRouteType(this SbmlModelCompartment compartment) {
            return compartment.GetCompartmentType().GetExposureRoute();
        }
    }
}
