using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {
    public class SbmlPbkModelCompartment {

        /// <summary>
        /// The SBML model species definition.
        /// </summary>
        public SbmlModelCompartment SbmlModelCompartment { get; set; }

        /// <summary>
        /// Volume (or mass) unit of the compartment.
        /// </summary>
        public ConcentrationMassUnit Unit { get; set; }

        /// <summary>
        /// Gets/sets the parameter id.
        /// </summary>
        public string Id => SbmlModelCompartment.Id;

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name => SbmlModelCompartment.Name;

        /// <summary>
        /// The compartment type.
        /// </summary>
        public PbkModelCompartmentType CompartmentType => GetCompartmentType();

        /// <summary>
        /// The biological matrix of the compartment, derived from the compartment type.
        /// </summary>
        public BiologicalMatrix BiologicalMatrix => GetBiologicalMatrix();

        public PbkModelCompartmentType GetCompartmentType() {
            var types = SbmlModelCompartment.BqmIsResources?
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
                throw new Exception($"Incorrect annotation for compartment {Id}: compartment is linked to multiple types.");
            } else if (types?.Count == 1) {
                return types.First();
            }
            return PbkModelCompartmentType.Undefined;
        }

        public BiologicalMatrix GetBiologicalMatrix() {
            var result = GetCompartmentType().GetBiologicalMatrix();
            return result;
        }

        public int GetExposureRouteInputPriority(ExposureRoute exposureRoute) {
            return GetCompartmentType().GetExposureRouteInputPriority(exposureRoute);
        }

        public int GetSystemicInputPriority() {
            return GetCompartmentType().GetSystemicInputPriority();
        }

        public ExposureRoute GetExposureRouteType() {
            return GetCompartmentType().GetExposureRoute();
        }
    }
}
