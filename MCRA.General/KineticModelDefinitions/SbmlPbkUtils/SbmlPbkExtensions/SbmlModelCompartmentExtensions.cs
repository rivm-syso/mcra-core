using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelCompartmentExtensions {

        public static PbkModelCompartmentType GetCompartmentType(this SbmlModelCompartment compartment) {
            var types = compartment.BqbIsResources?
                .Select(r => PbkModelCompartmentTypeConverter.FromUri(r, allowInvalidString: true))
                .Where(r => r != PbkModelCompartmentType.Undefined)
                .Distinct();
            if (types?.Any() ?? false) {
                // TODO: what to do when multiple types are found (e.g., parent/child terms)?
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

        public static ExposurePathType GetExposurePathType(this SbmlModelCompartment compartment) {
            return compartment.GetCompartmentType().GetExposureRoute().GetExposurePath();
        }
    }
}
