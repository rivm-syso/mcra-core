using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelParameterExtensions {

        public static PbkModelParameterType GetParameterType(this SbmlModelParameter parameter) {
            var types = parameter.BqbIsResources?
                .Select(r => PbkModelParameterTypeConverter.FromUri(r, allowInvalidString: true))
                .Where(r => r != PbkModelParameterType.Undefined)
                .Distinct();
            if (types != null && types.Any()) {
                // TODO: what to do when multiple types are found (e.g., parent/child terms)?
                return types.First();
            }
            return PbkModelParameterType.Undefined;
        }

        public static bool IsOfType(
            this SbmlModelParameter parameter,
            PbkModelParameterType type
        ) {
            // TODO: consider implementation of option for checking recursive
            // based on parent types
            var parameterType = GetParameterType(parameter);
            return parameterType == type;
        }
    }
}
